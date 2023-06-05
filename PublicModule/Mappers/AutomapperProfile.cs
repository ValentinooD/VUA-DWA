using AutoMapper;
using DAL.BLModels;
using DAL.Models;
using PublicModule.ViewModels;

namespace PublicModule.Mappers
{
    public class AutomapperProfile : Profile
    {
        public AutomapperProfile()
        {
            CreateMap<BLUser, VMUser>();
        }
    }
}
