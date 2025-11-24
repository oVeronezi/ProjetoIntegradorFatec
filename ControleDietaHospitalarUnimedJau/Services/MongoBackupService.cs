using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class MongoBackupService : BackgroundService
{
    private readonly ILogger<MongoBackupService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _mongodumpPath;

    public MongoBackupService(ILogger<MongoBackupService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        // Caminho do mongodump (mantenha o seu caminho exato aqui)
        _mongodumpPath = @"C:\mongodb-database-tools-windows-x86_64-100.13.0\mongodb-database-tools-windows-x86_64-100.13.0\bin\mongodump.exe";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Serviço de Backup Agendado Iniciado. 🕗");

        while (!stoppingToken.IsCancellationRequested)
        {
            // Lógica do Agendamento (inalterada)
            var config = _configuration.GetSection("BackupConfig");
            int targetHour = config.GetValue<int>("ExecutionHour");
            int targetMinute = config.GetValue<int>("ExecutionMinute");

            var now = DateTime.Now;
            var nextRun = new DateTime(now.Year, now.Month, now.Day, targetHour, targetMinute, 0);

            if (now > nextRun)
            {
                nextRun = nextRun.AddDays(1);
            }

            var delay = nextRun - now;
            _logger.LogInformation($"Próximo backup agendado para: {nextRun} (faltam {delay.TotalHours:F1} horas).");

            // Aguarda
            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            // Executa Backup, Compactação e Limpeza
            try
            {
                PerformBackupAndZipAndCleanup(); // <- Mudei a chamada do método
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao tentar realizar o backup.");
            }
        }
    }

    // Método principal renomeado para refletir todas as funcionalidades
    private void PerformBackupAndZipAndCleanup()
    {
        var config = _configuration.GetSection("BackupConfig");
        string dbName = config["DatabaseName"];
        string basePath = config["DirectoryPath"];

        string folderName = DateTime.Now.ToString("yyyy-MM-dd_HHmm");
        string dumpDirectoryPath = Path.Combine(basePath, folderName);
        string zipFilePath = Path.Combine(basePath, $"{folderName}.zip");
        int retentionDays = config.GetValue<int>("RetentionDays", 30); // Pega o valor da config

        if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);

        // --- 1. EXECUTAR MONGODUMP ---
        _logger.LogInformation($"Iniciando processo de mongodump na pasta: {dumpDirectoryPath} 💾");

        var processInfo = new ProcessStartInfo
        {
            FileName = _mongodumpPath,
            Arguments = $"--db {dbName} --out \"{dumpDirectoryPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = Process.Start(processInfo))
        {
            process.WaitForExit();
            string output = process.StandardError.ReadToEnd();

            if (process.ExitCode != 0)
            {
                _logger.LogError($"Falha no backup. ExitCode: {process.ExitCode}. Detalhes: {output}");
                return;
            }
        }

        // --- 2. COMPACTAR PARA ZIP ---
        _logger.LogInformation("Backup BSON concluído. Iniciando compactação ZIP... 📁");

        try
        {
            ZipFile.CreateFromDirectory(dumpDirectoryPath, zipFilePath, CompressionLevel.Fastest, false);
            _logger.LogInformation($"Compactação concluída com sucesso em: {zipFilePath}");

            // --- 3. LIMPAR ARQUIVOS TEMPORÁRIOS ---
            _logger.LogInformation($"Removendo pasta temporária: {dumpDirectoryPath}");
            Directory.Delete(dumpDirectoryPath, recursive: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao compactar/limpar a pasta temporária do backup. Verifique permissões.");
        }

        // --- 4. LIMPEZA DE ARQUIVOS ANTIGOS (NOVA FUNCIONALIDADE) ---
        CleanupOldBackups(basePath, retentionDays);
    }

    // NOVO MÉTODO PARA LIMPAR ARQUIVOS ZIP ANTIGOS
    private void CleanupOldBackups(string basePath, int retentionDays)
    {
        _logger.LogInformation($"Iniciando limpeza de backups com mais de {retentionDays} dias. 🗑️");

        // Calcula a data limite para retenção (ex: 30 dias atrás)
        var retentionDate = DateTime.Now.AddDays(-retentionDays);

        // Busca todos os arquivos .zip no diretório e filtra os antigos
        var oldFiles = Directory.EnumerateFiles(basePath, "*.zip")
                                .Where(f => File.GetCreationTime(f) < retentionDate);

        int count = 0;
        foreach (var file in oldFiles)
        {
            try
            {
                File.Delete(file);
                _logger.LogInformation($"Arquivo de backup antigo deletado: {Path.GetFileName(file)}");
                count++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao deletar o arquivo antigo {Path.GetFileName(file)}. Verifique permissões.");
            }
        }
        _logger.LogInformation($"Limpeza concluída. {count} arquivos antigos deletados. ✨");
    }
}