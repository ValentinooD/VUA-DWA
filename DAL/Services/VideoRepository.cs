using AutoMapper;
using DAL.BLModels;
using DAL.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace DAL.Services
{
    public class VideoRepository : IVideoRepository
    {
        private readonly IMapper mapper;
        private readonly RwaMoviesContext ctx;

        public VideoRepository(RwaMoviesContext ctx, IMapper mapper)
        {
            this.ctx = ctx;
            this.mapper = mapper;
        }

        public IEnumerable<BLVideo> GetAll()
        {
            var videos = ctx.Videos;
            var blVideos = mapper.Map<IEnumerable<BLVideo>>(videos);

            return blVideos;
        }

        public IEnumerable<BLVideo> GetFilteredData(string term)
        {
            var videos = ctx.Videos.Where(x =>
                x.Name.Contains(term) ||
                !x.Description.IsNullOrEmpty() ? x.Description.Contains(term) : false);
            var blVideos = mapper.Map<IEnumerable<BLVideo>>(videos);

            return blVideos;
        }

        public IEnumerable<BLVideo> GetPagedAndSearchedData(int page, int size, string search)
        {
            IEnumerable<Video> list;

            if (string.IsNullOrEmpty(search))
            {
                list = ctx.Videos;
            } else
            {
                list = ctx.Videos.Where(x => x.Name.ToLower().Contains(search.ToLower()));
            }

            /*if (!string.IsNullOrEmpty(search))
                list = list.Where(x => x.Name.ToLower().Contains(search.ToLower())); */

            list = list.Skip(page * size).Take(size);
            var blVideos = mapper.Map<IEnumerable<BLVideo>>(list);

            return blVideos;
        }

        public IEnumerable<BLVideo> GetPagedData(int page, int size, string orderBy, string direction)
        {
            // All of this should go to repository
            IEnumerable<Video> list = ctx.Videos.AsEnumerable();

            // Ordering
            if (string.Compare(orderBy, "id", true) == 0)
            {
                list = list.OrderBy(x => x.Id);
            }
            else if (string.Compare(orderBy, "name", true) == 0)
            {
                list = list.OrderBy(x => x.Name);
            }
            else if (string.Compare(orderBy, "description", true) == 0)
            {
                list = list.OrderBy(x => x.Description);
            }
            else
            {
                // default: order by Id
                list = list.OrderBy(x => x.Id);
            }

            if (string.Compare(direction, "desc", true) == 0)
            {
                list = list.Reverse();
            }

            list = list.Skip(page * size).Take(size);
            var blVideos = mapper.Map<IEnumerable<BLVideo>>(list);

            return blVideos;
        }

        public int GetTotalCount()
        {
            return ctx.Videos.Count();
        }
    }
}
