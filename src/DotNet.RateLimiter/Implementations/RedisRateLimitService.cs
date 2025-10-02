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

        // Lua script for atomic INCR with conditional EXPIRE
        // This ensures the counter is incremented and TTL is set atomically
        private const string LuaScript = @"
            local current = redis.call('INCR', KEYS[1])
            if current == 1 then
                redis.call('EXPIRE', KEYS[1], ARGV[1])
            end
            return current
        ";

        public RedisRateLimitService(ILogger<RedisRateLimitService> logger,
            IDatabase database)
        {
            _logger = logger;
            _database = database;
        }

        public async Task<bool> HasAccessAsync(string resourceKey, int periodInSec, int limit)
        {
            // Execute Lua script to atomically increment counter and set TTL on first request
            var count = (long)await _database.ScriptEvaluateAsync(
                LuaScript,
                new RedisKey[] { resourceKey },
                new RedisValue[] { periodInSec }
            );

            // Check if rate limit is exceeded
            if (count > limit)
            {
                _logger.LogCritical($"DotNet.RateLimiter:: key: {resourceKey} - count: {count}");
                return false;
            }

            return true;
        }
    }
}