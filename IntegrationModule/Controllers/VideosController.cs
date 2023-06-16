using DAL.Models;
using IntegrationModule.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IntegrationModule.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    [EnableCors("API")]
    public class VideosController : ControllerBase
    {
        private readonly RwaMoviesContext ctx;

        public VideosController(RwaMoviesContext ctx)
        {
            this.ctx = ctx;
        }

        [HttpGet("[action]")]
        public ActionResult<IEnumerable<Video>> GetAll() 
        {
            return Ok(ctx.Videos);
        }

        [HttpGet("[action]")]
        public ActionResult<Video> Get(int id)
        {
            try
            {
                var video = ctx.Videos.FirstOrDefault(x => x.Id == id);
                if (video == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }

                return Ok(video);
            } catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("[action]")]
        public ActionResult<Video> Create([FromBody] VideoCreateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int? genreId = ctx.Genres.FirstOrDefault(x => x.Name.Equals(request.GenreName)).Id;

                if (!genreId.HasValue)
                {
                    return BadRequest();
                }

                Video video = new Video()
                {
                    Name = request.Name,
                    Description = request.Description,
                    GenreId = genreId.Value,
                    CreatedAt = DateTime.Now,
                    StreamingUrl = request.StreamingUrl,
                    TotalSeconds = request.TotalSeconds
                };

                ctx.Videos.Add(video);
                ctx.SaveChanges();

                return Ok(video);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("[action]")]
        public ActionResult<Video> Edit(int id, [FromBody] VideoCreateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                
                Video video = ctx.Videos.FirstOrDefault(x => x.Id == id);
                if (video == null)
                {
                    return NotFound();
                }

                int? genreId = ctx.Genres.FirstOrDefault(x => x.Name.Equals(request.GenreName)).Id;

                if (!genreId.HasValue)
                {
                    return BadRequest();
                }

                video.Name = request.Name;
                video.Description = request.Description;
                video.GenreId = genreId.Value;
                video.CreatedAt = DateTime.Now;
                video.StreamingUrl = request.StreamingUrl;
                video.TotalSeconds = request.TotalSeconds;

                ctx.SaveChanges();

                return Ok(video);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        [HttpGet("[action]")]
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
            } catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        



    }
}
