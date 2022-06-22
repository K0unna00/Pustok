using FluentEmail.Core;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pustok.DAL;
using Pustok.Models;
using Pustok.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using MimeKit;
using MimeKit.Text;
using Newtonsoft.Json;

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

                    string token = await _userManager.GenerateEmailConfirmationTokenAsync(member);
                    var url = Url.Action("ConfirmEmail", "Account", new
                    {
                        email = member.Email,
                        token = token
                    }, Request.Scheme);
                    //SendEmail(url);
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
                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", "Input Correct Informations");
                        return View();
                    }
                    else
                    {
                        if (!member.EmailConfirmed)
                        {
                            ModelState.AddModelError("", "Please Verify Your Email");
                            return View();
                        }
                        return RedirectToAction("index", "Home");
                    }
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
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(MemberForgotPasswordViewModel memberVM)
        {
            if (!ModelState.IsValid)
                return View();
            AppUser member = await _userManager.Users.FirstOrDefaultAsync(x => x.NormalizedEmail == memberVM.Email.ToUpper());
            if (member == null)
            {
                ModelState.AddModelError("", "Type Correct Datas !");
                return View();
            }
            string token =await _userManager.GeneratePasswordResetTokenAsync(member);
            var url = Url.Action("ResetPassword", "Home", new { email = member.Email, token = token },Request.Scheme);
            return Ok(new { Url = url });
        }
        public IActionResult ResetPassword(string email , string token)
        {
            MemberResetPasswordViewModel Vm = new MemberResetPasswordViewModel()
            {
                Email = email,
                Token = token
            };

            return View(Vm);
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(MemberResetPasswordViewModel Vm)
        {
            AppUser user =await _userManager.FindByEmailAsync(Vm.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Type Correct Datas !");
                return View();
            }
            var result = await _userManager.ResetPasswordAsync(user, Vm.Token, Vm.ConfirmPassword);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "home");
            }
            ModelState.AddModelError("", "Someting Wrong");
            return View();
            
        }
        public IActionResult BookDetail(int Id)
        {
            BookDetailViewModel Vm = new BookDetailViewModel()
            {
                Book = _context.Books
                .Include(x => x.Author)
                .Include(x => x.BookImages)
                .Include(x => x.Genre)
                .Include(x => x.BookTags)
                .FirstOrDefault(x => x.Id == Id)
            };
            return View();
        }
        public IActionResult SetCookie()
        {
            HttpContext.Response.Cookies.Append("name", "hikmet");
            return Content("");
        }
        public IActionResult GetCookie()
        {
            var value=HttpContext.Request.Cookies["Basket"];
            return Content(value);
        }
        public IActionResult AddBasket(int Id)
        {
            List < BasketItemViewModel > basketItems= null;
            string basketJson = Request.Cookies["Basket"];
            if (basketJson == null)
            {
                basketItems = new List<BasketItemViewModel>();
            }
            else
            {
                basketItems = JsonConvert.DeserializeObject<List<BasketItemViewModel>>(basketJson);
            }
            var basketItem = basketItems.FirstOrDefault(x => x.BookId == Id);
            if (basketItem == null)
            {
                basketItem = new BasketItemViewModel
                {
                    Count=1,
                    BookId=Id
                };
            }
            else
            {
                basketItem.Count++;
            }
            basketItems.Add(basketItem);
            string newBasketJson = JsonConvert.SerializeObject(basketItems);
            Response.Cookies.Append("Basket",newBasketJson);
            return RedirectToAction("index", "Home");
        }

        //public void SendEmail(string url)
        //{
        //    var email = new MimeMessage();
        //    email.From.Add(MailboxAddress.Parse("bexi@gmail.com"));
        //    email.To.Add(MailboxAddress.Parse("78pf4jz@code.edu.az"));
        //    email.Subject = "Thanks for register out Website";
        //    email.Body = new TextPart(TextFormat.Html) { Text = $@"<a href={url} >Salam</a>" };

        //    // send email
        //    using var smtp = new MailKit.Net.Smtp.SmtpClient();
        //    smtp.Connect("smtp.ethereal.email", 587, SecureSocketOptions.StartTls);
        //    smtp.Authenticate("username", "passwird");
        //    smtp.Send(email);
        //    smtp.Disconnect(true);
        //    System.Web.Mail.MailMessage msg = new System.Web.Mail.MailMessage();
        //    msg.Body = message.Body;

        //    string smtpServer = "mail.business.it";
        //    string userName = "username";
        //    string password = "password";
        //    int cdoBasic = 1;
        //    int cdoSendUsingPort = 2;
        //    if (userName.Length > 0)
        //    {
        //        msg.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpserver", smtpServer);
        //        msg.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpserverport", 25);
        //        msg.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusing", cdoSendUsingPort);
        //        msg.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate", cdoBasic);
        //        msg.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusername", userName);
        //        msg.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendpassword", password);
        //    }
        //    msg.To = message.Destination;
        //    msg.From = "me@domain.it";
        //    msg.Subject = message.Subject;
        //    msg.BodyFormat = MailFormat.Html;//System.Text.Encoding.UTF8;
        //    SmtpMail.SmtpServer = smtpServer;
        //    SmtpMail.Send(msg);
        //}
    }
}
