using AutoMapper;
using DAL.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PublicModule.ViewModels;

namespace PublicModule.Controllers
{
    public class UserController : Controller
    {
        private IUserRepository repo;
        private IMapper mapper;

        public UserController(IUserRepository repo, IMapper mapper)
        {
            this.repo = repo;
            this.mapper = mapper;
        }
        
        public IActionResult Index()
        {
            var blUsers = repo.GetAll();
            var vmUsers = mapper.Map<IEnumerable<VMUser>>(blUsers);

            return View(vmUsers);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(VMRegister register)
        {
            if (!ModelState.IsValid)
                return View(register);

            var user = repo.CreateUser(
                register.Username,
                register.FirstName,
                register.LastName,
                register.Email,
                register.Password);

            return RedirectToAction("Index");
        }

        public IActionResult ValidateEmail(VMValidateEmail validateEmail)
        {
            if (!ModelState.IsValid)
                return View(validateEmail);

            // Confirm email, skip BL for simplicity
            repo.ConfirmEmail(
                validateEmail.Email,
                validateEmail.SecurityToken);

            return RedirectToAction("Index");
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

            var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Email) };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties()).Wait();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();

            return RedirectToAction("Index");
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(VMChangePassword changePassword)
        {
            repo.ChangePassword(
                changePassword.Username,
                changePassword.Password,
                changePassword.NewPassword);

            return RedirectToAction("Index");
        }
    }
}
