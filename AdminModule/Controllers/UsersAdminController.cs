using AdminModule.ViewModel;
using AutoMapper;
using DAL.BLModels;
using DAL.Models;
using DAL.Models.Requests;
using DAL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using PublicModule.ViewModels;

namespace AdminModule.Controllers
{
    public class UsersAdminController : Controller
    {
        private RwaMoviesContext ctx;
        private IUserRepository userRepo;
        private IMapper mapper;

        public UsersAdminController(RwaMoviesContext ctx, IUserRepository userRepo, IMapper mapper)
        {
            this.ctx = ctx;
            this.userRepo = userRepo;
            this.mapper = mapper;
        }

        // GET: UsersController
        public ActionResult Index()
        {
            return View(userRepo.GetAll());
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
        public ActionResult Create(VMCreate form)
        {
            if (!ModelState.IsValid)
                return View(form);

            int countryId = ctx.Countries.FirstOrDefault(x => x.Name.Equals(form.CountryName)).Id;

            var user = userRepo.CreateUser(
                form.Username,
                form.FirstName,
                form.LastName,
                form.Email,
                form.Password,
                countryId);

            user.IsConfirmed = true;
            ctx.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: UsersController/Edit/5
        public ActionResult Edit(int id)
        {
            return View(userRepo.GetUser(id));
        }

        // POST: UsersController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, VMUser user)
        {
            try
            {
                userRepo.Edit(id, mapper.Map<BLUser>(user));
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
            return View(userRepo.GetUser(id));
        }

        // POST: UsersController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                userRepo.SoftDeleteUser(id);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
