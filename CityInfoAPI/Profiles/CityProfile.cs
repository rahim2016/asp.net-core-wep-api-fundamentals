using AutoMapper;

namespace CityInfoAPI.Profiles
{
    public class CityProfile : Profile
    {
        public CityProfile()
        {
            CreateMap<Entities.City, Models.CityWithoutPointOfInterestDto>();
            CreateMap<Entities.City, Models.CityDto>();
            CreateMap<Models.CityDto, Entities.City>();
            //CreateMap<Models.CityWithoutPointOfInterestDto, Entities.City>();
            CreateMap<Models.CreateCityDto, Entities.City>();
        }
    }
}
