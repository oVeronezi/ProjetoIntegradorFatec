using Microsoft.AspNetCore.Identity;
using ControleDietaHospitalarUnimedJau.Data;
using ControleDietaHospitalarUnimedJau.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ControleDietaHospitalarUnimedJau.Models;
namespace ControleDietaHospitalarUnimedJau
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Add Services no container
            builder.Services.AddControllersWithViews();
            //builder.Services.AddDbContext<ControleDietaHospitalarUnimedJauContext>(options =>
            //options.UseSqlServer(builder.Configuration.GetConnectionString("ControleDietaHospitalarUnimedJauContext") ?? throw new InvalidOperationException("Connection string 'ControleDietaHospitalarUnimedJauContext' not found.")));

            builder.Services.AddControllersWithViews();

            //conexão com o mongodb
            ContextMongodb.ConnectionString = builder.Configuration.GetSection("MongoConnection:ConnectionString").Value;
            ContextMongodb.Database = builder.Configuration.GetSection("MongoConnection:Database").Value;
            ContextMongodb.IsSSL = Convert.ToBoolean(builder.Configuration.GetSection("MongoConnection:Isssl").Value);

            //configuração Identity
            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>
                (ContextMongodb.ConnectionString, ContextMongodb.Database)
                .AddDefaultTokenProviders();

            //configuração do envio email
            //builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            //builder.Services.AddSingleton<EmailService>();

            builder.Services.AddScoped<ContextMongodb>();

            var app = builder.Build();
            ////lógica da assinatura no evento
            //using (var scope = app.Services.CreateScope())
            //{
            //    var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
            //    //assinante
            //    ProjetoMongoDb.Services.EventoNotifier.OnParticipanteRegistrado += emailService.HandleRegistroAsync;
            //}

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

            app.Run();
        }
    }
}