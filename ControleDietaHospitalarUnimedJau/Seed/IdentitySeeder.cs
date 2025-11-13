using ControleDietaHospitalarUnimedJau.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
namespace ProjetoMongoDb.Seed
{


    public static class IdentitySeeder
    {
        public static async Task SeedRolesAndAdminUser(IServiceProvider serviceProvider, string defaultPassword)
        {
            // Obtendo os Managers através da Injeção de Dependência
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            const string adminRole = "Administrador";
            const string adminEmail = "admin@seudominio.com"; // Email padrão do administrador

            // --- PARTE 1: GARANTIR QUE A ROLE EXISTA ---

            if (await roleManager.FindByNameAsync(adminRole) == null)
            {
                var result = await roleManager.CreateAsync(new ApplicationRole { Name = adminRole });
                if (result.Succeeded)
                {
                    Console.WriteLine($"[SEED] Role '{adminRole}' criada com sucesso.");
                }
            }
            else
            {
                Console.WriteLine($"[SEED] Role '{adminRole}' já existe.");
            }

            // --- PARTE 2: CRIAR O USUÁRIO ADMINISTRADOR ---

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    NomeCompleto = "Administrador Master", // Adicione o nome
                    EmailConfirmed = true
                };

                // O UserManager faz o HASH da senha de forma segura
                IdentityResult createResult = await userManager.CreateAsync(adminUser, defaultPassword);

                if (createResult.Succeeded)
                {
                    // Atribui a Role "Administrador" ao usuário
                    await userManager.AddToRoleAsync(adminUser, adminRole);
                    Console.WriteLine($"[SEED] Usuário Admin '{adminEmail}' criado e role atribuída.");
                }
            }
            else
            {
                Console.WriteLine($"[SEED] Usuário Admin '{adminEmail}' já existe.");
            }
        }
    }
}
