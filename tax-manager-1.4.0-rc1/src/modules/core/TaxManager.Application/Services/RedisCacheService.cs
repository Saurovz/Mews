using System.Text.Json;
using Microsoft.Extensions.Logging;
using TaxManager.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace TaxManager.Application.Services;

public class RedisCacheService: ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IDistributedCache cache,ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }
  
    public async Task<T> GetAsync<T>(string key)
    {
        try
        {
            var cachedData = await _cache.GetStringAsync(key);
            return (cachedData == null) ? default : JsonSerializer.Deserialize<T>(cachedData, _jsonOptions);
            
            //if (cachedData == null) return default;
            //return JsonSerializer.Deserialize<T>(cachedData, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis get operation failed for key {Key}", key);
            return default;
        }
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var options = new DistributedCacheEntryOptions();
            if (expiry.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiry;
            }

            var serializedData = JsonSerializer.Serialize(value, _jsonOptions);
            await _cache.SetStringAsync(key, serializedData, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis set operation failed for key {Key}", key);
        }
    }
   
    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis delete operation failed for key {Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            var data = await _cache.GetAsync(key);
            return data != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis exists operation failed for key {Key}", key);
            return false;
        }
    }
    }
