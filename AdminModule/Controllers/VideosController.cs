using AdminModule.Models.Requests;
using AdminModule.ViewModel;
using Azure.Core;
using DAL.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Server;

namespace AdminModule.Controllers
{

    
    public class VideosController : Controller
    {
        private RwaMoviesContext ctx;

        public VideosController(RwaMoviesContext ctx)
        {
            this.ctx = ctx;
        }

        public ActionResult Index()
        {
            ViewData["videos"] = ctx.Videos.ToList();
            return View();
        }

        [HttpGet()]
        public ActionResult<List<Video>> Search(string? name, int? page, int? size, string? orderBy)
        {
            try
            {
                var videos = ctx.Videos.Where(x => true); // ToLower() because we ignore case;

                if (!name.IsNullOrEmpty())
                {
                    videos = ctx.Videos.Where(x => x.Name.ToLower().Contains(name.ToLower())); // ToLower() because we ignore case
                }

                if (videos.IsNullOrEmpty())
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }

                if (page.HasValue && size.HasValue)
                {
                    videos = videos.Skip(page.Value * size.Value).Take(size.Value);
                }

                if (!orderBy.IsNullOrEmpty())
                {
                    if (string.Compare(orderBy, "id", true) == 0)
                    {
                        videos = videos.OrderBy(x => x.Id);
                    }
                    else if (string.Compare(orderBy, "name", true) == 0)
                    {
                        videos = videos.OrderBy(x => x.Name);
                    }
                    else if (string.Compare(orderBy, "totalseconds", true) == 0)
                    {
                        videos = videos.OrderBy(x => x.TotalSeconds);
                    }
                }

                return Ok(videos.ToList());
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // GET: VideosController/Details/5
        public ActionResult Details(int id)
        {
            ViewData["video"] = ctx.Videos.FirstOrDefault(x => x.Id == id);
            return View();
        }

        // GET: VideosController/Create
        [HttpGet]
        public ActionResult Create()
        {
            ViewData["genres"] = ctx.Genres.ToList();
            return View(new Video());
        }

        // POST: VideosController/Create
        [HttpPost]
//        [ValidateAntiForgeryToken]
        //public ActionResult Create(IFormCollection collection)
        public IActionResult Create([FromForm] VMVideo request)
        {
            try
            {
                int genreId = ctx.Genres.FirstOrDefault(x => x.Name.Equals(request.Genre)).Id;

                int? imageId = null;
                var imageArray = GetFileByteAray(request.Image);
                if (request.Image != null)
                {
                    Image image = new Image()
                    {
                        Content = Convert.ToBase64String(imageArray)
                    };

                    ctx.Images.Add(image);
                    ctx.SaveChanges();

                    imageId = image.Id;
                }

                Video video = new Video()
                {
                    Name = request.Name,
                    Description = request.Description,
                    GenreId = genreId,
                    StreamingUrl = request.StreamingUrl,
                    TotalSeconds = 0,
                    ImageId = imageId
                };
                ctx.Videos.Add(video);
                ctx.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ViewData["genres"] = ctx.Genres.ToList();
                return View(new Video());
            }
        }

        // GET: VideosController/Edit/5
        public ActionResult Edit(int id)
        {
            ViewData["genres"] = ctx.Genres.ToList();
            Video video = ctx.Videos.FirstOrDefault(x => x.Id == id);
            if (video == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            return View(video);
        }

        // POST: VideosController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [FromForm] VMVideo form)
        {
            try
            {
                Video video = ctx.Videos.FirstOrDefault(x =>x.Id == id);
                if (video == null)
                {
                    return BadRequest();
                }

                int genreId = ctx.Genres.FirstOrDefault(x => x.Name.Equals(form.Genre)).Id;

                video.Name = form.Name;
                video.Description = form.Description;
                video.GenreId = genreId;

                ctx.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ViewData["genres"] = ctx.Genres.ToList();
                Video video = ctx.Videos.FirstOrDefault(x => x.Id == id);
                if (video == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }

                return View();
            }
        }

        // GET: VideosController/Delete/5
        public ActionResult Delete(int id)
        {
            var model = ctx.Videos.FirstOrDefault(x => x.Id == id);
            if (model == null) return NotFound();

            return View(model);
        }

        // POST: VideosController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                var model = ctx.Videos.FirstOrDefault(x => x.Id == id);
                if (model == null) return NotFound();

                ctx.Videos.Remove(model);
                ctx.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        private static byte[] GetFileByteAray(IFormFile formFile)
        {
            if (formFile != null)
            {
                if (formFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        formFile.CopyTo(memoryStream);

                        if (memoryStream.Length < 8 * 1024 * 1024)
                        {
                            return memoryStream.ToArray();
                        }
                    }

                }
            }

            return null;
        }
    }
}
