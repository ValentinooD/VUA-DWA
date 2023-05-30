using AdminModule.ViewModel;
using Azure.Core;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;

namespace AdminModule.Controllers
{
    public class TagsController : Controller
    {
        private RwaMoviesContext ctx;

        public TagsController(RwaMoviesContext ctx)
        {
            this.ctx = ctx;
        }

        // GET: TagsController
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet()]
        public ActionResult<List<Video>> List(int? page, int? size)
        {
            try
            {
                var tags = ctx.Tags.Where(x => true);

                if (page.HasValue && size.HasValue)
                {
                    tags = tags.Skip(page.Value * size.Value).Take(size.Value);
                }

                return Ok(tags.ToList());
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // GET: TagsController/Details/5
        public ActionResult Details(int id)
        {
            var model = ctx.Tags.FirstOrDefault(x => x.Id == id);

            if (model == null) return NotFound();

            return View(model);
        }

        // GET: TagsController/Create
        public ActionResult Create()
        {
            return View(new Tag());
        }

        // POST: TagsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([FromForm] VMTag form)
        {
            try
            {
                Tag tag = new Tag()
                {
                    Name = form.Name
                };
                ctx.Tags.Add(tag);
                ctx.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TagsController/Edit/5
        public ActionResult Edit(int id)
        {
            var model = ctx.Tags.FirstOrDefault(x => x.Id == id);
            if (model == null) return NotFound();

            return View(model);
        }

        // POST: TagsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [FromForm] VMTag form)
        {
            try
            {
                var model = ctx.Tags.FirstOrDefault(x => x.Id == id);
                if (model == null) return NotFound();

                model.Name = form.Name;
                ctx.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                var model = ctx.Tags.FirstOrDefault(x => x.Id == id);
                if (model == null) return NotFound();
                return View(model);
            }
        }

        // GET: TagsController/Delete/5
        public ActionResult Delete(int id)
        {
            var model = ctx.Tags.FirstOrDefault(x => x.Id == id);
            if (model == null) return NotFound();

            return View(model);
        }

        // POST: TagsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                var model = ctx.Tags.FirstOrDefault(x => x.Id == id);
                if (model == null) return NotFound();

                ctx.Tags.Remove(model);
                ctx.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                var model = ctx.Tags.FirstOrDefault(x => x.Id == id);
                if (model == null) return NotFound();
                return View(model);
            }
        }
    }
}
