using AutoMapper;
using DAL.BLModels;
using DAL.Models;

namespace DAL.Mappers
{
    public class AutomapperProfile : Profile
    {
        public AutomapperProfile()
        {
            CreateMap<User, BLUser>();
            CreateMap<Video, BLVideo>();
        }
    }
}
