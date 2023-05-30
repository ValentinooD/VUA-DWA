using DAL.Models;
using DAL.Models.Requests;
using DAL.Services;
using Microsoft.AspNetCore.Mvc;
using PublicModule.Models.Requests;

namespace PublicModule.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserRepository repository;

        public HomeController(IUserRepository repository)
        {
            this.repository = repository;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login([FromForm] UserLoginRequest form)
        {
            try
            {
                


                return RedirectToAction(nameof(Index));
            } catch (InvalidOperationException ex)
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult<User> Register([FromBody] UserRegisterRequest request)
        {
            try
            {
                var newUser = repository.Create(request);
                repository.ValidateEmail(new ValidateEmailRequest()
                {
                    Username = newUser.Username,
                    B64SecToken = newUser.SecurityToken
                });


                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ViewData["error"] = ex.Message;
                return View();
            }
        }
    }
}