using AutoMapper;
using Microsoft.Extensions.Logging;
using TaxManager.Application.Common;
using TaxManager.Application.Dto;
using TaxManager.Application.Interfaces;
using TaxManager.Application.Common.Exception;
using TaxManager.Domain.Entities;
using TaxManager.Domain.Interfaces;

namespace TaxManager.Application.Services;

public class CountryService(IMapper mapper, ICountryRepository repository, 
    ILogger<CountryService> logger,
    ICacheService redis) : ICountryService
{
    public async Task<IEnumerable<CountryDto>> GetAllCountriesAsync()
    {
        // Get the cache key 
        const string cacheKey = CacheKeys.CountryAll;
        
        // Try to get from cache first
        var cachedCountries = await redis.GetAsync<IEnumerable<CountryDto>>(cacheKey);
        if (cachedCountries != null)
        {
            logger.LogInformation("Found countries from cache");
            return cachedCountries;
        }
        
        //If not in cache, get from database
        var countries = await repository.GetAllAsync();
        var entities =  mapper.Map<IEnumerable<CountryDto>>(countries).OrderBy(c => c.Name).ToList();

        if (entities == null || entities.Count == 0)
        {
            throw new NotFoundException("No Items found.");
        }
        
        //Cache the result for 20 mins, TTL is optional 
        await redis.SetAsync(cacheKey, entities, TimeSpan.FromMinutes(20));
        
        return entities;
    }

    public async Task<IEnumerable<SubdivisionDto>> GetSubdivisionsByCountryIdAsync(int countryId)
    {
        var subdivisions = await repository.GetSubdivisionsByCountryIdAsync(countryId);
        var entities =  mapper.Map<IEnumerable<SubdivisionDto>>(subdivisions).ToList();

        if (entities == null || entities.Count == 0)
        {
            throw new NotFoundException("No Items found.");
        }

        return entities;
    }

    public async Task<bool> DeleteSubdivisionByIdAsync(int id)
    {
        var subdivision = await repository.GetSubdivisionByIdAsync(id);
        if (subdivision == null)
        {
            throw new NotFoundException($"Entity with id '{id}' not found.");
        }
        
        var deleteResponse = await repository.DeleteSubdivisionAsync(subdivision);
        if (deleteResponse != 1)
        {
            throw new ServiceException("Failed to delete subdivision.");
        }

        return true;
    }
}
