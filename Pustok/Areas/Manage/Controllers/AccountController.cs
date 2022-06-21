using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Pustok.Areas.Manage.ViewModels;
using Pustok.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Pustok.Areas.Manage.Controllers
{
    [Area("manage")]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> CreateSuperAdmin()
        {
            AppUser admin = new AppUser
            {
                UserName = "SuperAdmin",
                Fullname = "Super Admin",
                IsAdmin = true

            };

            var result = await _userManager.CreateAsync(admin, "Admin123");
            await _userManager.AddToRoleAsync(admin, "SuperAdmin");
            if (!result.Succeeded)
            {
                return Ok(result.Errors.ToList());
            }
            return View();
        }
        public IActionResult Roles()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }
        public IActionResult CreateRole()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateRole(IdentityRole role)
        {
            await _roleManager.CreateAsync(new IdentityRole(role.Name));
            return RedirectToAction("index", "account");
        }
        public IActionResult Index()
        {
            var data = _userManager.Users.ToList();
            return View(data);
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(AdminLoginViewModel admin)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "UserName or Password is not Correct");
                return View();
            }

            AppUser user = await _userManager.FindByNameAsync(admin.UserName);
            if (user == null)
            {
                ModelState.AddModelError("", "UserName or Password is not Correct");
                return View();
            }
            var result = await _signInManager.PasswordSignInAsync(user, admin.Password, false, false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "UserName or Password is not Correct");
                return View();
            }
            if (user.IsAdmin)
            {
                return RedirectToAction("index", "dashboard");
            }
            else
            {
                ModelState.AddModelError("", "Join with Admin Account!!");
                return View();
            }

        }
        public IActionResult CreateAdmin()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateAdmin(AdminRegisterViewModel admin)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            if (admin.ConfirmPassword != admin.Password)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords doesnt match !");
                return View();
            }
            else
            {
                AppUser user = new AppUser()
                {
                    UserName = admin.UserName,
                    Fullname = admin.FullName,
                    Email = admin.Email,
                    IsAdmin = true
                };

                var result = await _userManager.CreateAsync(user, admin.Password);
                await _userManager.AddToRoleAsync(user, "Admin");
                if (!result.Succeeded)
                {
                    return Ok(result.Errors);
                }
                return RedirectToAction("Index", "Account");
            }

        }

        //public async Task<IActionResult> EditAdmin(AdminRegisterViewModel admin)
        //{
        //    var adminExists = await _userManager.FindByNameAsync(admin.UserName);
            
        //}

        public async Task<IActionResult> AdminLogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
        public async Task<IActionResult> ConfirmEmail(string email , string token)
        {
            AppUser user =await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Content("xx");
            }
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return RedirectToAction("index");
            }
            return Content("Error");
        }
    }
}
