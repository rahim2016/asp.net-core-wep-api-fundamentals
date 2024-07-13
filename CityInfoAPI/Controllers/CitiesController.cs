using Asp.Versioning;
using AutoMapper;
using CityInfoAPI.Entities;
using CityInfoAPI.Models;
using CityInfoAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CityInfoAPI.Controllers
{

    [Route("api/v{version:apiVersion}/cities")]
    [Authorize]
    [Produces("application/json", "application/xml")]
    [ApiController]
    [ApiVersion(1)]
    public class CitiesController : ControllerBase
    {
        private readonly CitiesDataStore _citiesDataStore;
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;

        const int MaxPageSize = 20;

        public CitiesController(ICityInfoRepository cityInfoRepository, IMapper mapper)
        {
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get a list of cities.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CityWithoutPointOfInterestDto>), 200)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<CityWithoutPointOfInterestDto>>> GetCities(string? name, string? searchQuery, int pageNumber = 1, int pageSize = 10)
        {
            if (pageSize > MaxPageSize)
            {
                pageSize = MaxPageSize;
            }

            var (cities, paginationMetadata) = await _cityInfoRepository.GetCitiesAsyn(name, searchQuery, pageNumber, pageSize);

            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            return Ok(_mapper.Map<IEnumerable<CityWithoutPointOfInterestDto>>(cities));
        }

        /// <summary>
        /// Get a specific City with or without point of interest.
        /// </summary>
        /// <param name="id">The id of the city to get</param>
        /// <param name="includePointsOfInterest">Whether or not to include the pointd of interest</param>
        /// <returns>A city with or without points of interest</returns>
        /// <response code="200">Returns the requested city</response>
        [HttpGet("{cityId}", Name = "GetCity")]
        [ProducesResponseType(typeof(CityWithoutPointOfInterestDto), 200)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCity(int cityId, bool includePointsOfInterest = false)
        {
            var city = await _cityInfoRepository.GetCityAsync(cityId, includePointsOfInterest);

            if (city == null)
            {
                return NotFound();
            }

            if (includePointsOfInterest)
            {
                return Ok(_mapper.Map<CityDto>(city));
            }

            return Ok(_mapper.Map<CityWithoutPointOfInterestDto>(city));
        }


        /// <summary>
        /// Creates a City.
        /// </summary>
        /// <param name="cityDto"></param>
        /// <returns>A newly created City</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /City
        ///     {
        ///        "Name": "New York City",
        ///        "Description": "The one with that big park."
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Returns the newly created city</response>
        /// <response code="400">If the city is null</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CityWithoutPointOfInterestDto>> CreateCity(CreateCityDto cityDto)
        {
            var city = await _cityInfoRepository.CityExistByNameAsync(cityDto.Name);
            if (city)
            {
                return BadRequest("City already exists");
            }

            var cityEntity = _mapper.Map<City>(cityDto);

            var createdCity = await _cityInfoRepository.AddCity(cityEntity);

            if (createdCity == null)
            {
                return StatusCode(500, "A problem happened while handling your request");
            }

            var cityToReturn = _mapper.Map<CityWithoutPointOfInterestDto>(createdCity);

            return CreatedAtRoute("GetCity", new { id = cityToReturn.Id }, cityToReturn);
        }
    }
}
