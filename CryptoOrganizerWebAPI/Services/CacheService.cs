using CryptoOrganizerWebAPI.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace CryptoOrganizerWebAPI.Services
{
    public class CacheService(IMemoryCache cache) : ICacheService
    {
        public void Set<T>(string key, T data, TimeSpan? expiry = null)
        {
            var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(expiry ?? TimeSpan.FromMinutes(5));

            cache.Set(key, data, options);
        }

        public T Get<T>(string key) => cache.TryGetValue(key, out T data) ? data : default;

        public void Remove(string key) => cache.Remove(key);

        public bool Exists(string key) => cache.TryGetValue(key, out _);
    }
}