using DAL.Models;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationModule.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        private readonly RwaMoviesContext ctx;

        public GenresController(RwaMoviesContext ctx)
        {
            this.ctx = ctx;
        }

        [HttpGet()]
        public ActionResult<IEnumerable<Genre>> GetAll()
        {
            return ctx.Genres.ToList();
        }

        [HttpGet("{id}")]
        public ActionResult<Genre> Get(int id)
        {
            try
            {
                var genre = ctx.Genres.FirstOrDefault(x => x.Id == id);
                if (genre == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }

                return genre;
            } catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        
        [HttpPost()]
        public ActionResult<Genre> Create(string name)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                Genre genre = ctx.Genres.FirstOrDefault(x => x.Name.Equals(name));
                if (genre != null)
                {
                    return Ok(genre);
                }

                genre = new Genre() { Name = name };
                ctx.Genres.Add(genre);
                ctx.SaveChanges();

                return Ok(genre);
            } catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        
        [HttpDelete("{id}")]
        public ActionResult<Genre> Delete(int id)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);


                Genre genre = ctx.Genres.FirstOrDefault(x => x.Id == id);
                if (genre == null)
                {
                    return NotFound();
                }

                ctx.Genres.Remove(genre);
                ctx.SaveChanges();

                return Ok();
            } catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        [HttpPut("{id}")]
        public ActionResult<Genre> Edit(int id, [FromBody] Genre genre)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);


                Genre dbGenre = ctx.Genres.FirstOrDefault(x => x.Id == id);
                if (dbGenre == null)
                {
                    return NotFound();
                }

                genre.Id = dbGenre.Id; // The ID in the request doesn't matter, but we'll update it when returning
                dbGenre.Name = genre.Name;
                dbGenre.Description = genre.Description;

                ctx.SaveChanges();

                return Ok(genre);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
