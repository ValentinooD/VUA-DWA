using AdminModule.ViewModel;
using DAL.BLModels;
using DAL.Models;
using DAL.Models.Requests;
using DAL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AdminModule.Controllers
{
    public class UsersAdminController : Controller
    {
        private IUserRepository userRepo;

        public UsersAdminController(IUserRepository userRepo)
        { 
            this.userRepo = userRepo;
        }

        // GET: UsersController
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet()]
        public ActionResult<List<Video>> Search(string? name, int? page, int? size)
        {
            try
            {
                var list = userRepo.GetAll().Where(x => true);

                if (!name.IsNullOrEmpty())
                {
                    list = userRepo.GetAll().Where(x => x.Username.ToLower().Contains(name.ToLower())); // ToLower() because we ignore case
                }

                if (list.IsNullOrEmpty())
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }

                if (page.HasValue && size.HasValue)
                {
                    list = list.Skip(page.Value * size.Value).Take(size.Value);
                }

                return Ok(list.ToList());
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // GET: UsersController/Details/5
        public ActionResult Details(int id)
        {
            var model = userRepo.GetAll().FirstOrDefault(x => x.Id == id);

            if (model == null) return NotFound();

            return View(model);
        }

        // GET: UsersController/Create
        public ActionResult Create()
        {
            return View(new User());
        }

        // POST: UsersController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([FromForm] UserRegisterRequest form)
        {
            try
            {
                BLUser user = userRepo.Create(form);
                userRepo.ValidateEmail(new ValidateEmailRequest()
                {
                    Username = form.Username,
                    B64SecToken = user.SecurityToken
                });

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: UsersController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: UsersController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: UsersController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UsersController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
