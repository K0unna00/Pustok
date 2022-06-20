using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pustok.DAL;
using Pustok.Models;
using Pustok.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Pustok.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public HomeController(AppDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            ViewBag.Sliders = _context.Sliders.ToList();
            HomeViewModel HWM = new HomeViewModel()
            {
                IsDiscounted = _context.Books.Include(x => x.Author).Include(x => x.BookImages).Where(x => x.DiscountPercent > 0).Take(20).ToList(),
                IsFeatured = _context.Books.Include(x => x.Author).Include(x => x.BookImages).Where(x => x.IsFeatured).Take(20).ToList(),
                IsNew = _context.Books.Include(x => x.Author).Include(x => x.BookImages).Where(x => x.IsNew).Take(20).ToList()
            };
            return View(HWM);
        }
        public IActionResult GetBookDetail(int id)
        {
            Book book = _context.Books.Include(x => x.Genre).Include(x => x.Author).Include(x => x.BookImages).FirstOrDefault(x => x.Id == id);

            return PartialView("_BookModalPartial", book);
        }
        public IActionResult MemberRegister()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> MemberRegister(MemberRegisterViewModel user)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            else
            {
                if (user.Password == user.ConfirmPassword)
                {

                    AppUser member = new AppUser()
                    {
                        UserName = user.UserName,
                        Fullname = user.FullName,
                        Email = user.Email,
                        IsAdmin = false
                    };
                    var result = await _userManager.CreateAsync(member, user.Password);
                    await _userManager.AddToRoleAsync(member,"Member");
                    if (!result.Succeeded)
                    {
                        return Ok(result.Errors);
                    }
                    return RedirectToAction("MemberLogin", "Home");
                }
                ModelState.AddModelError("", "Passwords Not Macking");

                return View();

            }
        }
        public IActionResult MemberLogin()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> MemberLogin(MemberLoginViewModel user)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            else
            {
                AppUser member = await _userManager.FindByNameAsync(user.UserName);
                if (member != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(member, user.Password, false, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("index", "Home");
                    }
                    ModelState.AddModelError("", "Input Correct Informations");
                    return View();
                }
                ModelState.AddModelError("", "Input Correct Informations");
                return View();

            }
        }
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }

    }
}
