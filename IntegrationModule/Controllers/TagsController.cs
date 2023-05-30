using DAL.Models;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationModule.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly RwaMoviesContext ctx;

        public TagsController(RwaMoviesContext ctx)
        {
            this.ctx = ctx;
        }

        [HttpGet()]
        public ActionResult<IEnumerable<Tag>> GetAll()
        {
            return ctx.Tags.ToList();
        }

        [HttpGet("{id}")]
        public ActionResult<Tag> Get(int id)
        {
            try
            {
                var tag = ctx.Tags.FirstOrDefault(x => x.Id == id);
                if (tag == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }

                return tag;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        [HttpPost()]
        public ActionResult<Tag> Create(string name)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                Tag tag = ctx.Tags.FirstOrDefault(x => x.Name.Equals(name));
                if (tag != null)
                {
                    return Ok(tag);
                }

                tag = new Tag() { Name = name };
                ctx.Tags.Add(tag);
                ctx.SaveChanges();

                return Ok(tag);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("{id}")]
        public ActionResult<Tag> Delete(int id)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);


                Tag tag = ctx.Tags.FirstOrDefault(x => x.Id == id);
                if (tag == null)
                {
                    return NotFound();
                }

                ctx.Tags.Remove(tag);
                ctx.SaveChanges();

                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        [HttpPut("{id}")]
        public ActionResult<Tag> Edit(int id, [FromBody] Tag tag)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);


                Tag dbTag = ctx.Tags.FirstOrDefault(x => x.Id == id);
                if (dbTag == null)
                {
                    return NotFound();
                }

                tag.Id = dbTag.Id; // The ID in the request doesn't matter, but we'll update it when returning
                dbTag.Name = tag.Name;

                ctx.SaveChanges();

                return Ok(tag);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
