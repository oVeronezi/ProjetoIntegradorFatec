using ControleDietaHospitalarUnimedJau.Data;
using ControleDietaHospitalarUnimedJau.Models;
using ControleDietaHospitalarUnimedJau.Services;
using ControleDietaHospitalarUnimedJau.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjetoMongoDb.Seed;

namespace ControleDietaHospitalarUnimedJau
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =================================================================
            // 🚀 SEÇÃO 1: REGISTRO DE SERVIÇOS (TUDO ANTES DE 'builder.Build()')
            // =================================================================

            // 1. Configuração do MVC
            builder.Services.AddControllersWithViews();

            // 2. Configuração do DbContext (SQL Server)
            builder.Services.AddDbContext<ControleDietaHospitalarUnimedJauContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ControleDietaHospitalarUnimedJauContext")
                ?? throw new InvalidOperationException("Connection string 'ControleDietaHospitalarUnimedJauContext' not found.")));

            // 3. Configuração da Conexão com o MongoDB (String de Conexão)
            ContextMongodb.ConnectionString = builder.Configuration.GetSection("MongoConnection:ConnectionString").Value;
            ContextMongodb.Database = builder.Configuration.GetSection("MongoConnection:Database").Value;
            ContextMongodb.IsSSL = Convert.ToBoolean(builder.Configuration.GetSection("MongoConnection:Isssl").Value);

            // 4. Configuração do Identity (Autenticação com MongoDB)
            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>
                (ContextMongodb.ConnectionString, ContextMongodb.Database)
                .AddDefaultTokenProviders();

            // 5. Configuração de Cookies de Autenticação (MOVIDO DA SEÇÃO 'app')
            builder.Services.ConfigureApplicationCookie(options =>
            {
                // Rota usada quando o usuário TENTA ACESSAR uma página protegida SEM ESTAR LOGADO.
                options.LoginPath = "/Account/Login";

                // Rota usada quando o usuário ESTÁ LOGADO, mas não tem a permissão necessária.
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            // 6. Configuração e Registro do Serviço de E-mail (CRÍTICO: Resolve o erro de injeção)
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

            // Registro do EmailService. Se você tem IEmailService, use AddSingleton<IEmailService, EmailService>().
            // Se você só tem a classe concreta (como estava no erro original), use:
            builder.Services.AddSingleton<EmailService>();

            // 7. Registro do Contexto MongoDB e Factory de Claims
            builder.Services.AddScoped<ContextMongodb>();
            builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>>();

            // =================================================================
            // 🏗️ SEÇÃO 2: CONSTRUÇÃO E CONFIGURAÇÃO DO PIPELINE HTTP (APÓS 'builder.Build()')
            // =================================================================

            var app = builder.Build(); // A partir daqui, não se registra mais serviços.

            // **NOVO BLOCO:** Lógica de Assinatura do Evento e Seed
            using (var scope = app.Services.CreateScope())
            {
                // Obtém a instância do EmailService (Agora ele está registrado e funciona!)
                var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

                // --- LÓGICA DE SEEDING DO ADMINISTRADOR ---
                var defaultAdminPassword = app.Configuration["AdminSettings:DefaultPassword"]
                    ?? throw new InvalidOperationException("AdminSettings:DefaultPassword não configurada.");

                // Chama o método de seed para criar a Role e o Usuário Admin
                await IdentitySeeder.SeedRolesAndAdminUser(scope.ServiceProvider, defaultAdminPassword);
                // --- FIM DA LÓGICA DE SEEDING ---
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            Rotativa.AspNetCore.RotativaConfiguration.Setup(app.Environment.WebRootPath, "Rotativa");
            app.Run();
        }
    }
}