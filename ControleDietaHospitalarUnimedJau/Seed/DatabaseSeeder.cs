using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
// Não há necessidade deste using. O GetUserManager() é geralmente um método de extensão.
// using MongoDB.Driver.Core.Operations; 

// Nota: Se a classe estiver na pasta Seed, ela deve ter um namespace como: 
namespace ControleDietaHospitalarUnimedJau.Seed;
public class DatabaseSeeder
{
    private readonly IMongoDatabase _database;
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _dbName;

    public DatabaseSeeder(IMongoClient client, IConfiguration configuration, ILogger<DatabaseSeeder> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Obtém o nome do banco da configuração
        _dbName = configuration.GetValue<string>("BackupConfig:DatabaseName")
                  ?? configuration.GetValue<string>("mongoConnection:Database");

        if (string.IsNullOrEmpty(_dbName))
        {
            throw new InvalidOperationException("Nome do banco de dados não configurado.");
        }

        _database = client.GetDatabase(_dbName);
    }

    public async Task SeedRolesAndUsersAsync()
    {
        _logger.LogInformation("Iniciando configuração de segurança do MongoDB...");

        try
        {
            // 1. Criar Roles com Permissões Granulares
            await CreateRolesAsync();

            // 2. Criar Usuário Admin (Emergência)
            await CreateInitialAdminUserAsync();

            // 3. Criar Usuário de Serviço (Aplicação)
            await CreateIdentityServiceUserAsync();

            _logger.LogInformation("Configuração de segurança concluída com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante a execução do Seeder. Verifique a conexão com o banco.");
            throw; // Relança para que o Program.cs saiba que falhou
        }
    }

    private async Task CreateRolesAsync()
    {
        // Role: Administrador (Total)
        await EnsureRoleExistsAsync("Administrador", new BsonArray {
            new BsonDocument("role", "dbOwner"),
            new BsonDocument("role", "userAdmin")
        });

        // Role: IdentityService (Backend C# - Necessário para o Identity funcionar)
        await EnsureRoleExistsAsync("IdentityService", new BsonArray {
            new BsonDocument("role", "readWrite"),
            new BsonDocument("role", "userAdmin") // Necessário para criar usuários via UserController
        });

        // Role: Nutricionista (CRUD Dietas/Entregas, Leitura Pacientes)
        await EnsureRoleExistsAsync("Nutricionista", new BsonArray {
            CreatePrivilege("Dietas", new BsonArray { "find", "insert", "update", "remove" }),
            CreatePrivilege("Entregas", new BsonArray { "find", "insert", "update", "remove" }),
            CreatePrivilege("Pacientes", new BsonArray { "find" }),
            CreatePrivilege("Copeiras", new BsonArray { "find" }),
            CreatePrivilege("Bandejas", new BsonArray { "find" })
        });

        // Role: Copeira (Leitura Apenas)
        await EnsureRoleExistsAsync("Copeira", new BsonArray {
            CreatePrivilege("Pacientes", new BsonArray { "find" }),
            CreatePrivilege("Dietas", new BsonArray { "find" }),
            CreatePrivilege("Entregas", new BsonArray { "find" }),
            CreatePrivilege("Bandejas", new BsonArray { "find" })
        });
    }

    private BsonDocument CreatePrivilege(string collection, BsonArray actions)
    {
        return new BsonDocument
        {
            { "resource", new BsonDocument("db", _dbName).Add("collection", collection) },
            { "actions", actions }
        };
    }

    private async Task EnsureRoleExistsAsync(string roleName, BsonArray privileges)
    {
        try
        {
            // Comando para verificar se a role já existe no banco (consulta de informação funciona bem)
            var rolesInfo = await _database.RunCommandAsync((Command<BsonDocument>)$"{{ rolesInfo: 1, filter: {{ role: \"{roleName}\" }} }}");

            // Se a role não for encontrada, cria
            if (!rolesInfo.Contains("roles") || rolesInfo["roles"].AsBsonArray.Count == 0)
            {
                // CORRIGIDO: Especificando explicitamente o tipo de retorno como <BsonDocument> 
                // para resolver o erro CS0411 (Argumentos de tipo não podem ser inferidos).
                await _database.RunCommandAsync<BsonDocument>(new BsonDocument {
                    { "createRole", roleName },
                    { "privileges", privileges },
                    { "roles", new BsonArray() }
                });
                _logger.LogWarning($"Role '{roleName}' criada.");
            }
        }
        catch (Exception ex)
        {
            // Loga o erro, geralmente acontece se o usuário conectado não tem permissão de leitura/criação de roles
            _logger.LogWarning($"Verificação da role {roleName} falhou: {ex.Message}");
        }
    }

    private async Task CreateInitialAdminUserAsync()
    {
        await CreateUserAsync("AdminUser", "Administrador");
    }

    private async Task CreateIdentityServiceUserAsync()
    {
        await CreateUserAsync("IdentityService", "IdentityService");
    }

    private async Task CreateUserAsync(string configSection, string roleName)
    {
        var username = _configuration[$"{configSection}:Username"];
        var password = _configuration[$"{configSection}:Password"];

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            _logger.LogWarning($"Credenciais para {configSection} não encontradas no appsettings.");
            return;
        }

        try
        {
            // 1. Comando para verificar se o usuário já existe (consulta de informação funciona bem)
            var userInfo = await _database.RunCommandAsync((Command<BsonDocument>)$"{{ usersInfo: 1, filter: {{ user: \"{username}\" }} }}");

            // Se o usuário não for encontrado, cria
            if (!userInfo.Contains("users") || userInfo["users"].AsBsonArray.Count == 0)
            {
                // CORRIGIDO: Especificando explicitamente o tipo de retorno como <BsonDocument> 
                // para resolver o erro CS0411 (Argumentos de tipo não podem ser inferidos).
                await _database.RunCommandAsync<BsonDocument>(new BsonDocument {
                    { "createUser", username },
                    { "pwd", password },
                    { "roles", new BsonArray { new BsonDocument("role", roleName).Add("db", _dbName) } }
                });

                _logger.LogCritical($"Usuário '{username}' criado com sucesso.");
            }
        }
        catch (Exception ex)
        {
            // Loga o erro caso a criação do usuário falhe
            _logger.LogError(ex, $"Erro CRÍTICO ao criar usuário {username}. Verifique as permissões de conexão.");
        }
    }
}