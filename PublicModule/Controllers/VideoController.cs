using AutoMapper;
using DAL.Models;
using DAL.Services;
using MessagePack.Formatters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PublicModule.ViewModels;
using System.Drawing;

namespace PublicModule.Controllers
{
    [Authorize]
    public class VideoController : Controller
    {
        private RwaMoviesContext ctx;
        private IMapper mapper;
        private IVideoRepository repo;

        public VideoController(RwaMoviesContext ctx, IVideoRepository repo, IMapper mapper)
        {
            this.ctx = ctx;
            this.repo = repo;
            this.mapper = mapper;
        }

        public IActionResult Index(int page, int size, string search)
        {
            // Set up some default values
            if (size == 0)
                size = 10;

            IEnumerable<VMVideo> videos = mapper.Map<IEnumerable<VMVideo>>(repo.GetPagedAndSearchedData(page, size, search));
            foreach (VMVideo video in videos)
            {
                video.Image = ctx.Images.FirstOrDefault(x => x.Id == video.ImageId);
                video.Genre = ctx.Genres.FirstOrDefault(x => x.Id == video.GenreId);
            }

            ViewData["page"] = page;
            ViewData["size"] = size;
            ViewData["search"] = search;
            ViewData["pages"] = repo.GetTotalCount() / size;

            return View(videos);
        }

        public IActionResult PagedVideos(int page, int size, string search)
        {
            // Set up some default values
            if (size == 0)
                size = 10;

            IEnumerable<VMVideo> videos = mapper.Map<IEnumerable<VMVideo>>(repo.GetPagedAndSearchedData(page, size, search));
            foreach (VMVideo video in videos)
            {
                video.Image = ctx.Images.FirstOrDefault(x => x.Id == video.ImageId);
                video.Genre = ctx.Genres.FirstOrDefault(x => x.Id == video.GenreId);
            }

            ViewData["page"] = page;
            ViewData["size"] = size;
            ViewData["search"] = search;
            ViewData["pages"] = repo.GetTotalCount() / size;

            return PartialView("_PartialVideoEntry", videos);
        }

        public IActionResult Watch(int id)
        {
            VMVideo video = mapper.Map<VMVideo>(ctx.Videos.FirstOrDefault(x => x.Id == id));
            video.Image = ctx.Images.FirstOrDefault(x => x.Id == video.ImageId);
            video.Genre = ctx.Genres.FirstOrDefault(x => x.Id == video.GenreId);

            return View(video);
        }
    }
}
