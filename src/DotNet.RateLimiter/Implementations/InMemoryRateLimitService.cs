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
            var period = TimeSpan.FromSeconds(periodInSec);

            using var _ = await _lockProvider.LockAsync(resourceKey).ConfigureAwait(false);

            var now = DateTime.UtcNow;

            if (_memoryCache.TryGetValue(resourceKey, out InMemoryRateLimitEntry entry) &&
                entry is not null &&
                entry.Expiration > now)
            {
                if (++entry.Total >= limit)
                {
                    //rate limit exceeded
                    _logger.LogCritical("DotNet.RateLimiter:: key: {Key} - count: {Count}", resourceKey, entry.Total);
                    return false;
                }

                _memoryCache.Set(resourceKey, entry, period);
                return true;
            }

            _memoryCache.Set(resourceKey, new InMemoryRateLimitEntry
            {
                Total = 0,
                Expiration = now + period
            }, period);

            return true;
        }
    }
}