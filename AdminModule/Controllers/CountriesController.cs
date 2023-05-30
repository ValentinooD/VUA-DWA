using DAL.Models;
using Microsoft.AspNetCore.Mvc;

namespace AdminModule.Controllers
{
    public class CountriesController : Controller
    {
        private RwaMoviesContext ctx;

        public CountriesController(RwaMoviesContext ctx)
        {
            this.ctx = ctx;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet()]
        public ActionResult<List<Video>> List(int? page, int? size)
        {
            try
            {
                var countries = ctx.Countries.Where(x => true); // ToLower() because we ignore case

                if (page.HasValue && size.HasValue)
                {
                    countries = countries.Skip(page.Value * size.Value).Take(size.Value);
                }

                return Ok(countries.ToList());
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
