using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ControleDietaHospitalarUnimedJau.Data;
namespace ControleDietaHospitalarUnimedJau
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<ControleDietaHospitalarUnimedJauContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ControleDietaHospitalarUnimedJauContext") ?? throw new InvalidOperationException("Connection string 'ControleDietaHospitalarUnimedJauContext' not found.")));

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles(); 

            app.UseRouting();
            app.UseAuthorization();
            // Rota
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}