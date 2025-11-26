using ControleDietaHospitalarUnimedJau.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ControleDietaHospitalarUnimedJau.Models;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ControleDietaHospitalarUnimedJau.Controllers
{
    public class UserController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private RoleManager<ApplicationRole> _roleManager;
        public UserController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Create(string role)
        {
            ViewBag.Role = role;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(User user, string role)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser appuser = new ApplicationUser();
                //tirar os espaços
                string userName = user.NomeCompleto.Replace(" ", "");
                //tirar todos os acentos
                var normalizedString = userName.Normalize(NormalizationForm.FormD);
                StringBuilder sb = new StringBuilder();
                foreach (char c in normalizedString)
                {
                    if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    {
                        sb.Append(c);
                    }
                }
                userName = sb.ToString().Normalize(NormalizationForm.FormC);
                //retirar tudo que não for letras e números
                userName = Regex.Replace(userName, @"[^a-zA-Z0-9\s]", "");
                Console.WriteLine(userName);

                appuser.UserName = userName;
                appuser.Email = user.Email;
                appuser.NomeCompleto = user.NomeCompleto;
                IdentityResult result = await _userManager.CreateAsync(appuser, user.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(appuser, role);
                    ViewBag.Message = "Usuário Cadastrado com sucesso";
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }

            }//if
            return View(user);
        }//fim create

        [Authorize(Roles = "Administrador")]
        public IActionResult CreateRole()
        {
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CreateRole(UserRole useRole)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await _roleManager.CreateAsync(
                    new ApplicationRole() { Name = useRole.RoleName });
                if (result.Succeeded)
                {
                    ViewBag.Message = "Perfil cadastrado com sucesso";
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View();
        }
    }
}
