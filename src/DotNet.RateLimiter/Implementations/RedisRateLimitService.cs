using DotNet.RateLimiter.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace DotNet.RateLimiter.Implementations
{
    public class RedisRateLimitService(
        ILogger<RedisRateLimitService> logger,
        IDatabase database)
        : IRateLimitService
    {
        // Lua script for atomic INCR with conditional EXPIRE
        // This ensures the counter is incremented and TTL is set atomically
        private const string LuaScript = @"
            local current = redis.call('INCR', KEYS[1])
            if current == 1 then
                redis.call('EXPIRE', KEYS[1], ARGV[1])
            end
            return current
        ";

        public async Task<bool> HasAccessAsync(string resourceKey, int periodInSec, int limit)
        {
            // Execute Lua script to atomically increment counter and set TTL on first request
            var count = (long)await database.ScriptEvaluateAsync(
                LuaScript,
                [resourceKey],
                [periodInSec]
            );

            // Check if rate limit is exceeded
            if (count > limit)
            {
                logger.LogCritical($"DotNet.RateLimiter:: key: {resourceKey} - count: {count}");
                return false;
            }

            return true;
        }
    }

}