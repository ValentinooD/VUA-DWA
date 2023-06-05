using AdminModule.ViewModel;
using AutoMapper;
using DAL.BLModels;
using DAL.Models;
using DAL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.IdentityModel.Tokens;
using System.Drawing;

namespace AdminModule.Controllers
{

    
    public class VideosController : Controller
    {
        private RwaMoviesContext ctx;
        private IVideoRepository repo;
        private IMapper mapper;

        public VideosController(IVideoRepository repo, RwaMoviesContext ctx, IMapper mapper)
        {
            this.ctx = ctx;
            this.repo = repo;
            this.mapper = mapper;
        }

        public ActionResult Index(int page, int size, string orderBy, string direction)
        {
            // Set up some default values
            if (size == 0)
                size = 10;

            var videos = repo.GetPagedData(page, size, orderBy, direction);

            ViewData["page"] = page;
            ViewData["size"] = size;
            ViewData["orderBy"] = orderBy;
            ViewData["direction"] = direction;
            ViewData["pages"] = repo.GetTotalCount() / size;

            return View(videos);
        }

        public IActionResult PartialVideos(int page, int size, string orderBy, string direction)
        {
            // Set up some default values
            if (size == 0)
                size = 10;

            var list = repo.GetPagedData(page, size, orderBy, direction);

            ViewData["page"] = page;
            ViewData["size"] = size;
            ViewData["orderBy"] = orderBy;
            ViewData["direction"] = direction;
            ViewData["pages"] = repo.GetTotalCount() / size;

            return PartialView("_PartialVideoEntry", list);
        }

        // GET: VideosController/Details/5
        public ActionResult Details(int id)
        {
            var video = ctx.Videos.FirstOrDefault(x => x.Id == id);
            if (video == null)
                return NotFound();

            return View(video);
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
                    DAL.Models.Image image = new DAL.Models.Image()
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
