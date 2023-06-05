using AdminModule.ViewModel;
using AutoMapper;
using DAL.Models;

namespace AdminModule.Mappers
{
    public class AutomapperProfile : Profile
    {
        public AutomapperProfile()
        {
            CreateMap<Video, VMVideo>();
        }
    }
}
