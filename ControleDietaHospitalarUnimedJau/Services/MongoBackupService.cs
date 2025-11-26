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

        // 1. Obter a String de Conexão
        string mongoUri = _configuration.GetValue<string>("mongoConnection:ConnectionString");

        if (string.IsNullOrEmpty(mongoUri))
        {
            _logger.LogError("Connection String não encontrada.");
            return;
        }

        string folderName = DateTime.Now.ToString("yyyy-MM-dd_HHmm");
        string gzFilePath = Path.Combine(basePath, $"{folderName}.gz");

        if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);

        _logger.LogInformation($"Iniciando backup direto via mongodump para: {gzFilePath} 💾");

        // --- 2. CORREÇÃO CRUCIAL NOS ARGUMENTOS ---
        // Em vez de redirecionar a saída (>), dizemos ao mongodump para escrever direto no arquivo (--archive=CAMINHO)
        // Isso é mais robusto e evita arquivos de 0kb por falha de stream.
        
        var processInfo = new ProcessStartInfo
        {
            FileName = _mongodumpPath,
            
            // AQUI ESTÁ O SEGREDO: --archive="CaminhoDoArquivo"
            Arguments = $"--uri=\"{mongoUri}\" --db {dbName} --archive=\"{gzFilePath}\" --gzip",
            
            RedirectStandardOutput = false, // Não precisamos ler a saída binária
            RedirectStandardError = true,   // Mas queremos ler os logs de erro/sucesso
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try 
        {
            using (var process = Process.Start(processInfo))
            {
                // Capturamos a saída de log do mongodump (ele escreve logs no Stderr)
                string outputLogs = process.StandardError.ReadToEnd();
                
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    _logger.LogInformation($"Backup concluído com sucesso! Arquivo criado: {gzFilePath}");
                    _logger.LogInformation($"Log do Mongodump: {outputLogs}"); // Útil para debug
                }
                else
                {
                    _logger.LogError($"Falha no mongodump (ExitCode: {process.ExitCode}). Detalhes:\n{outputLogs}");
                    
                    // Se falhou e criou um arquivo de 0kb, vamos apagá-lo para não confundir
                    if (File.Exists(gzFilePath) && new FileInfo(gzFilePath).Length == 0)
                    {
                        File.Delete(gzFilePath);
                    }
                }
            }
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Erro ao iniciar o processo mongodump.");
        }

        // --- 3. LIMPEZA ---
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