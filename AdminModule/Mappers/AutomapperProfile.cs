using AdminModule.ViewModel;
using AutoMapper;
using DAL.Models;

namespace Task11.Mappers
{
    public class AutomapperProfile : Profile
    {
        public AutomapperProfile()
        {
            CreateMap<Video, VMVideo>();
        }
    }
}
