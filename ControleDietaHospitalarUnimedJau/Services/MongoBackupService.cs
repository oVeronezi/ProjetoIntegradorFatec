using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// System.IO.Compression não é mais necessário

public class MongoBackupService : BackgroundService
{
    private readonly ILogger<MongoBackupService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _mongodumpPath;

    public MongoBackupService(ILogger<MongoBackupService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        // LÊ O CAMINHO DA CONFIGURAÇÃO (BackupConfig:MongodumpPath)
        _mongodumpPath = configuration.GetValue<string>("BackupConfig:MongodumpPath");

        if (string.IsNullOrEmpty(_mongodumpPath))
        {
            _logger.LogError("O caminho 'BackupConfig:MongodumpPath' não está configurado. O backup não será executado.");
        }
    }

    //----------------------------------------------------------------------------------

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Serviço de Backup Agendado Iniciado. 🕗");

        while (!stoppingToken.IsCancellationRequested)
        {
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

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            try
            {
                PerformBackupGzipAndCleanup();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao tentar realizar o backup.");
            }
        }
    }

    //----------------------------------------------------------------------------------

    private void PerformBackupGzipAndCleanup()
    {
        if (string.IsNullOrEmpty(_mongodumpPath))
        {
            _logger.LogError("Não é possível executar o backup: Caminho do mongodump não configurado.");
            return;
        }

        var config = _configuration.GetSection("BackupConfig");
        string dbName = config["DatabaseName"];
        string basePath = config["DirectoryPath"];
        int retentionDays = config.GetValue<int>("RetentionDays", 30);

        // LEITURA DA URI DE CONEXÃO DO ARQUIVO DE CONFIGURAÇÃO (appsettings.json)
        string mongoUri = _configuration.GetValue<string>("mongoConnection:ConnectionString");

        if (string.IsNullOrEmpty(mongoUri))
        {
            _logger.LogError("Connection String (mongoConnection:ConnectionString) não encontrada na configuração. O backup falhará.");
            return;
        }

        string folderName = DateTime.Now.ToString("yyyy-MM-dd_HHmm");
        string gzFilePath = Path.Combine(basePath, $"{folderName}.gz");

        if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);

        // --- 1. EXECUTAR MONGODUMP E CAPTURAR A SAÍDA ---
        _logger.LogInformation($"Iniciando processo de mongodump e compactação GZIP para: {gzFilePath} 💾");

        var processInfo = new ProcessStartInfo
        {
            FileName = _mongodumpPath,

            // CORREÇÃO: Usamos --uri para garantir que o mongodump se conecte.
            // O caminho completo da URI deve estar entre aspas duplas, caso haja espaços ou caracteres especiais.
            Arguments = $"--uri \"{mongoUri}\" --db {dbName} --archive --gzip",

            RedirectStandardOutput = true,  // Necessário para ler o fluxo binário
            RedirectStandardError = true,
            UseShellExecute = false,        // Essencial para redirecionar streams
            CreateNoWindow = true
        };

        using (var process = Process.Start(processInfo))
        {
            // BLOCO DE REDIRECIONAMENTO: LÊ A SAÍDA BINÁRIA E ESCREVE DIRETAMENTE NO ARQUIVO .GZ
            using (var outputStream = process.StandardOutput.BaseStream)
            using (var fileStream = new FileStream(gzFilePath, FileMode.Create, FileAccess.Write))
            {
                outputStream.CopyTo(fileStream);
            }

            process.WaitForExit();
            string output = process.StandardError.ReadToEnd();

            if (process.ExitCode != 0)
            {
                _logger.LogError($"Falha no mongodump. ExitCode: {process.ExitCode}. Detalhes: {output}");
                return;
            }
        }

        _logger.LogInformation($"Backup concluído com sucesso em formato GZIP: {gzFilePath} ✨");

        // --- 2. LIMPEZA DE ARQUIVOS ANTIGOS ---
        CleanupOldBackups(basePath, retentionDays);
    }

    //----------------------------------------------------------------------------------

    private void CleanupOldBackups(string basePath, int retentionDays)
    {
        _logger.LogInformation($"Iniciando limpeza de backups com mais de {retentionDays} dias. 🗑️");

        var retentionDate = DateTime.Now.AddDays(-retentionDays);

        // Buscando por arquivos .gz
        var oldFiles = Directory.EnumerateFiles(basePath, "*.gz")
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