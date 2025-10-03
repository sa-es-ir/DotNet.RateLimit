using DotNet.RateLimiter.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace DotNet.RateLimiter.Implementations
{
    public class RedisRateLimitService : IRateLimitService
    {
        private readonly IDatabase _database;
        private readonly ILogger<RedisRateLimitService> _logger;

        public RedisRateLimitService(
            ILogger<RedisRateLimitService> logger,
            IDatabase database)
        {
            _logger = logger;
            _database = database;
        }

        public async Task<bool> HasAccessAsync(string resourceKey, int periodInSec, int limit)
        {
            var count = await _database.StringIncrementAsync(resourceKey);

            if (count == 1)
            {
                await _database.KeyExpireAsync(resourceKey, TimeSpan.FromSeconds(periodInSec));
            }

            if (count > limit)
            {
                _logger.LogCritical($"DotNet.RateLimiter:: key: {resourceKey} - count: {count}");
                return false;
            }

            return true;
        }
    }

}