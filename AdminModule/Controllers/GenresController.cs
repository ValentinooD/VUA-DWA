using AdminModule.ViewModel;
using DAL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AdminModule.Controllers
{
    public class GenresController : Controller
    {
        private RwaMoviesContext ctx;

        public GenresController(RwaMoviesContext ctx)
        {
            this.ctx = ctx;
        }

        // GET: GenresController
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet()]
        public ActionResult<List<Genre>> List(int? page, int? size)
        {
            try
            {
                var list = ctx.Genres.Where(x => true);

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

        // GET: GenresController/Details/5
        public ActionResult Details(int id)
        {
            var model = ctx.Tags.FirstOrDefault(x => x.Id == id);

            if (model == null) return NotFound();

            return View(model);
        }

        // GET: GenresController/Create
        public ActionResult Create()
        {
            return View(new Genre());
        }

        // POST: GenresController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([FromForm] VMGenre form)
        {
            try
            {
                Genre model = new Genre()
                {
                    Name = form.Name,
                    Description = form.Description
                };
                ctx.Genres.Add(model);
                ctx.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: GenresController/Edit/5
        public ActionResult Edit(int id)
        {
            var model = ctx.Genres.FirstOrDefault(x => x.Id == id);
            if (model == null) return NotFound();
            return View(model);
        }

        // POST: GenresController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [FromForm] VMGenre form)
        {
            try
            {
                var model = ctx.Genres.FirstOrDefault(x => x.Id == id);
                if (model == null) return NotFound();

                model.Name = form.Name;
                model.Description = form.Description;
                ctx.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                var model = ctx.Genres.FirstOrDefault(x => x.Id == id);
                if (model == null) return NotFound();
                return View(model);
            }
        }

        // GET: GenresController/Delete/5
        public ActionResult Delete(int id)
        {
            var model = ctx.Genres.FirstOrDefault(x => x.Id == id);
            if (model == null) return NotFound();
            return View(model);
        }

        // POST: GenresController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                var model = ctx.Genres.FirstOrDefault(x => x.Id == id);
                if (model == null) return NotFound();

                ctx.Genres.Remove(model);
                ctx.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                var model = ctx.Genres.FirstOrDefault(x => x.Id == id);
                if (model == null) return NotFound();
                return View(model);
            }
        }
    }
}
