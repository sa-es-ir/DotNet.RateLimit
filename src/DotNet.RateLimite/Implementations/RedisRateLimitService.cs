using System;
using System.Threading.Tasks;
using DotNet.RateLimit.Interfaces;
using DotNet.RateLimit.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace DotNet.RateLimit.Implementations
{
    public class RedisRateLimitService : IRateLimitService
    {
        private readonly IDatabase _database;
        private readonly ILogger<RedisRateLimitService> _logger;
        private readonly IRateLimitBackgroundTaskQueue _backgroundTaskQueue;
        private readonly IOptions<RateLimitOptions> _options;
        private readonly IMemoryCache _memoryCache;


        public RedisRateLimitService(ILogger<RedisRateLimitService> logger,
            IRateLimitBackgroundTaskQueue backgroundTaskQueue,
            IOptions<RateLimitOptions> options,
            IDatabase database, IMemoryCache memoryCache)
        {
            _logger = logger;
            _backgroundTaskQueue = backgroundTaskQueue;
            _options = options;
            _database = database;
            _memoryCache = memoryCache;
        }

        public async Task<bool> HasAccessAsync(string resourceKey, int periodInSec, int limit)
        {
            if (!_options.Value.EnableRateLimit)
                return true;

            var counts = await _database.SortedSetLengthAsync(resourceKey, ToUtcTimestamp(DateTime.Now.AddSeconds(-periodInSec)),
                ToUtcTimestamp(DateTime.Now.AddSeconds(periodInSec)));

            if (counts >= limit)
            {
                _logger.LogCritical($"Rate limit : key :{resourceKey} - count:{counts}");

                return false;
            }

            _backgroundTaskQueue.QueueBackgroundWorkItem(token => _database.SortedSetAddAsync(resourceKey, DateTime.Now.Ticks,
                ToUtcTimestamp(DateTime.Now), CommandFlags.FireAndForget));

            _backgroundTaskQueue.QueueBackgroundWorkItem(token => _database.KeyExpireAsync(resourceKey,
                TimeSpan.FromSeconds(periodInSec), CommandFlags.FireAndForget));


            return true;
        }

        private long ToUtcTimestamp(DateTime dateTime)
        {
            return (long)dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}