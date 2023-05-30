using AutoMapper;
using DAL.BLModels;
using DAL.Models;

namespace Task11.Mappers
{
    public class AutomapperProfile : Profile
    {
        public AutomapperProfile()
        {
            CreateMap<User, BLUser>();
        }
    }
}
