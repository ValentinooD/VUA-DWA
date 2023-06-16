using AutoMapper;
using DAL.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PublicModule.ViewModels;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using DAL.BLModels;
using System.Configuration;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http.Extensions;
using System.Web;

namespace PublicModule.Controllers
{
    [AllowAnonymous]
    public class UserController : Controller
    {
        private ILogger<UserController> logger;
        private RwaMoviesContext ctx;
        private IUserRepository repo;
        private IMapper mapper;
        private IConfiguration configuration;

        public UserController(ILogger<UserController> logger, RwaMoviesContext ctx, IUserRepository repo, IMapper mapper, IConfiguration configuration)
        {
            this.logger = logger;
            this.ctx = ctx;
            this.repo = repo;
            this.mapper = mapper;
            this.configuration = configuration;
        }

        [Authorize]
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return Redirect("/video/");
            } else
            {
                return RedirectToAction("Login");
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult Profile()
        {
            string strID = User.FindFirst("ID")?.Value;

            if (strID == null)
                return RedirectToAction("Index");

            var user = mapper.Map<VMUser>(repo.GetUser(int.Parse(strID)));

            return View(user);
        }

        public IActionResult Register()
        {
            ViewData["countries"] = ctx.Countries.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Register(VMRegister register)
        {
            if (!ModelState.IsValid)
                return View(register);

            int countryId = ctx.Countries.FirstOrDefault(x => x.Name.Equals(register.CountryName)).Id;

            var user = repo.CreateUser(
                register.Username,
                register.FirstName,
                register.LastName,
                register.Email,
                register.Password,
                countryId);

            SendEmail(user);

            return RedirectToAction("Index");
        }
        
        public IActionResult ValidateEmail(VMValidateEmail validateEmail)
        {
            if (!ModelState.IsValid)
                return View(validateEmail);

            try
            {
                // Confirm email, skip BL for simplicity
                repo.ConfirmEmail(
                    validateEmail.Email,
                    validateEmail.SecurityToken);

                return RedirectToAction("Login");
            } catch (Exception)
            {
                return RedirectToAction("Register");
            }
        }

        [HttpGet("login")]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost("login")]
        public IActionResult Login(VMLogin login)
        {
            if (!ModelState.IsValid)
                return View(login);

            var user = repo.GetConfirmedUser(
                login.Username,
                login.Password);

            if (user == null)
            {
                ModelState.AddModelError("Username", "Invalid username or password");
                return View(login);
            }

            var claims = new List<Claim> { 
                new Claim(ClaimTypes.Name, user.Email),
                new Claim("ID", user.Id + "")
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var properties = new AuthenticationProperties
            {
                IsPersistent = login.StaySignedIn,
            };
            HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                properties).Wait();

            if (login.RedirectUrl != null)
            {
                return Redirect(login.RedirectUrl);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [Authorize]
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();

            return RedirectToAction("Login");
        }

        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult ChangePassword(VMChangePassword changePassword)
        {
            string strID = User.FindFirst("ID")?.Value;

            if (strID == null)
                return RedirectToAction("Index");

            var user = mapper.Map<VMUser>(repo.GetUser(int.Parse(strID)));

            if (!repo.Authenticate(user.Username, changePassword.OldPassword)) {
                ModelState.AddModelError("OldPassword", "Invalid password");
                return View();
            }

            if (!changePassword.NewPassword.Equals(changePassword.ConfirmPassword))
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match");
                return View();
            }

            repo.ChangePassword(
                user.Username,
                changePassword.NewPassword);

            return RedirectToAction("Profile");
        }

        private void SendEmail(BLUser user)
        {
            try
            {
                var client = new SmtpClient(configuration.GetValue<string>("MailSender:smtpServer"), configuration.GetValue<int>("MailSender:smtpPort"));
                var sender = configuration.GetValue<string>("MailSender:sender");

                var mail = new MailMessage(
                            from: new MailAddress(sender),
                            to: new MailAddress(user.Email));

                mail.Subject = "[DWA] Email verification";
                mail.Body = "Dear " + user.Username + ",\n";
                mail.Body += "Please click on the link below to verify your email address and access our service.\n\n";
                mail.Body += "https://" + Request.Host + Url.Action("ValidateEmail") + "?Email=" + HttpUtility.UrlEncodeUnicode(user.Email) + "&SecurityToken=" + HttpUtility.UrlEncodeUnicode(user.SecurityToken) + "\n\n";
                mail.Body += "Best regards,\n";
                mail.Body += "The DWA team";

                client.Send(mail);
            }
            catch (Exception ex)
            {
                logger.LogWarning("Failed to send email for user " + user.Email);
                logger.LogWarning(ex.Message);
            }
        }
    }
}
