using AsyncKeyedLock;
using DotNet.RateLimiter.Interfaces;
using DotNet.RateLimiter.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DotNet.RateLimiter.Implementations
{
    public class InMemoryRateLimitService : IRateLimitService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<InMemoryRateLimitService> _logger;
        private readonly AsyncKeyedLocker<string> _lockProvider;

        public InMemoryRateLimitService(IMemoryCache memoryCache,
            ILogger<InMemoryRateLimitService> logger,
            AsyncKeyedLocker<string> lockProvider)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _lockProvider = lockProvider;
        }

        public async Task<bool> HasAccessAsync(string resourceKey, int periodInSec, int limit)
        {
            using (await _lockProvider.LockAsync(resourceKey).ConfigureAwait(false))
            {
                var cacheEntry = new InMemoryRateLimitEntry()
                {
                    Expiration = DateTime.UtcNow,
                    Total = 0
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

                        //rate limit exceeded
                        if (cacheEntry.Total >= limit)
                        {
                            _logger.LogCritical($"DotNet.RateLimiter:: key: {resourceKey} - count: {cacheEntry.Total}");

                            return false;
                        }

                        _memoryCache.Set(resourceKey, cacheEntry, TimeSpan.FromSeconds(periodInSec));

                        return true;
                    }
                }

                _memoryCache.Set(resourceKey, cacheEntry, TimeSpan.FromSeconds(periodInSec));
            }

            return true;
        }
    }
}