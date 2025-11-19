using Microsoft.Extensions.Caching.Memory;

namespace UserService.Helper;

public interface ICacheService
{
    Task<T> GetOrCreateAsync<T>(string cacheKey,Func<Task<T>> retrieveDataFunc,TimeSpan? slidingExpiration = null);
    Task Clear(string? key);   
    Task<bool> IsKeyExists(string key); 
}

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
   
    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
      
    }

    public async Task<T> GetOrCreateAsync<T>(
    string cacheKey,
    Func<Task<T>> retrieveDataFunc,
    TimeSpan? slidingExpiration = null)
    {
        if (!_cache.TryGetValue(cacheKey, out T cachedData))
        {
            cachedData = await retrieveDataFunc();
           
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                SlidingExpiration = slidingExpiration ?? TimeSpan.FromDays(1)
            };
            _cache.Set(cacheKey, cachedData, cacheEntryOptions);
        }
        return cachedData;
    }

    public async Task Clear(string? key)
    {
        if (!string.IsNullOrWhiteSpace(key))
        {
            _cache.Remove(key);          
        }       
    }    
    public async Task<bool> IsKeyExists(string key)
    {
        bool exists = true;    
        if (!_cache.TryGetValue(key,out exists))
        {
            exists = false;
        }
        return exists;
    }
}