using CityInfoAPI.Entities;

namespace CityInfoAPI.Services
{
    public interface ICityInfoRepository
    {
        Task<IEnumerable<City>> GetCitiesAsyn();
        Task<(IEnumerable<City>, PaginationMetadata)> GetCitiesAsyn(string? name, string? searchQuery, int pageNumber, int pageSize);
        Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest);
        Task<City> AddCity(City city);
        Task<bool> CityExistAsync(int cityId);
        Task<bool> CityExistByNameAsync(string name);
        Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsyn(int cityId);
        Task<PointOfInterest?> GetPointOfInterestForCityAsync(int cityId, int pointOfInterestId);
        Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest);
        void DeletePointOfInterest(PointOfInterest pointOfInterest);
        Task<bool> CityNameMatchesCityId(string? cityName, int cityId);
        Task<bool> SaveChangesAsync();
    }
}
