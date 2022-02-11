using System;
using System.Threading.Tasks;
using DotNet.RateLimiter.Interfaces;
using DotNet.RateLimiter.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DotNet.RateLimiter.Implementations
{
    public class InMemoryRateLimitService : IRateLimitService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<InMemoryRateLimitService> _logger;

        public InMemoryRateLimitService(IMemoryCache memoryCache,
            ILogger<InMemoryRateLimitService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public Task<bool> HasAccessAsync(string resourceKey, int periodInSec, int limit)
        {
            var cacheEntry = new InMemoryRateLimitEntry()
            {
                Expiration = DateTime.UtcNow,
                Total = 1
            };

            if (_memoryCache.TryGetValue(resourceKey, out InMemoryRateLimitEntry entry))
            {
                if (entry != null && entry.Expiration.AddSeconds(periodInSec) > DateTime.UtcNow)
                {
                    cacheEntry = new InMemoryRateLimitEntry()
                    {
                        Expiration = entry.Expiration,
                        Total = entry.Total + 1
                    };

                    _memoryCache.Set(resourceKey, cacheEntry, TimeSpan.FromSeconds(periodInSec));

                    //rate limit exceeded
                    if (cacheEntry.Total > limit)
                    {
                        _logger.LogCritical($"Rate limit : key :{resourceKey} - count:{cacheEntry.Total}");

                        return Task.FromResult(false);
                    }



                    return Task.FromResult(true);
                }
            }

            _memoryCache.Set(resourceKey, cacheEntry, TimeSpan.FromSeconds(periodInSec));

            return Task.FromResult(true);
        }
    }
}