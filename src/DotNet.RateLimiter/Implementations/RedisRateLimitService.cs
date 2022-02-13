using System;
using System.Threading.Tasks;
using DotNet.RateLimiter.Interfaces;
using Microsoft.Extensions.Logging;
using RedLockNet;
using StackExchange.Redis;

namespace DotNet.RateLimiter.Implementations
{
    public class RedisRateLimitService : IRateLimitService
    {
        private readonly IDatabase _database;
        private readonly ILogger<RedisRateLimitService> _logger;
        private readonly IRateLimitBackgroundTaskQueue _backgroundTaskQueue;
        private readonly IDistributedLockFactory _lockFactory;
        private readonly TimeSpan _lockExpiry = TimeSpan.FromSeconds(300);
        private readonly TimeSpan _lockWait = TimeSpan.FromSeconds(120);
        private readonly TimeSpan _lockRetry = TimeSpan.FromMilliseconds(500);

        public RedisRateLimitService(ILogger<RedisRateLimitService> logger,
            IRateLimitBackgroundTaskQueue backgroundTaskQueue,
            IDatabase database,
            IDistributedLockFactory lockFactory)
        {
            _logger = logger;
            _backgroundTaskQueue = backgroundTaskQueue;
            _database = database;
            _lockFactory = lockFactory;
        }

        public async Task<bool> HasAccessAsync(string resourceKey, int periodInSec, int limit)
        {
            await using var distributedLock = await _lockFactory.CreateLockAsync(resourceKey, _lockExpiry, _lockWait, _lockRetry);

            if (distributedLock.IsAcquired)
            {
                var counts = await _database.SortedSetLengthAsync(resourceKey,
                    ToUtcTimestamp(DateTime.UtcNow.AddSeconds(-periodInSec)),
                    ToUtcTimestamp(DateTime.UtcNow.AddSeconds(periodInSec)));

                if (counts >= limit)
                {
                    _logger.LogCritical($"Rate limit : key :{resourceKey} - count:{counts}");

                    return false;
                }

                _backgroundTaskQueue.QueueBackgroundWorkItem(token => _database.SortedSetAddAsync(resourceKey,
                    DateTime.UtcNow.Ticks,
                    ToUtcTimestamp(DateTime.UtcNow), CommandFlags.FireAndForget));

                _backgroundTaskQueue.QueueBackgroundWorkItem(token => _database.KeyExpireAsync(resourceKey,
                    TimeSpan.FromSeconds(periodInSec), CommandFlags.FireAndForget));
            }

            return true;
        }

        private long ToUtcTimestamp(DateTime dateTime)
        {
            return (long)dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}