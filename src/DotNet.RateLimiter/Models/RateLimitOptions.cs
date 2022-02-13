using System.Collections.Generic;

namespace DotNet.RateLimiter.Models
{
    public class RateLimitOptions
    {
        /// <summary>
        /// if false will bypass all requests, default is true.
        /// </summary>
        public bool EnableRateLimit { get; set; } = true;

        /// <summary>
        /// in case of rate limit exceeded this status code will be exposed, default is 429.
        /// </summary>
        public int HttpStatusCode { get; set; } = 429;

        /// <summary>
        /// when rate limit exceeded this message will return, default is 'Rate Limit Exceeded'.
        /// </summary>
        public string ErrorMessage { get; set; } = "Rate limit Exceeded";

        /// <summary>
        /// if redis connection present then use redis for rate limit
        /// </summary>
        public bool HasRedis => !string.IsNullOrEmpty(RedisConnection?.Trim());

        /// <summary>
        /// redis connection
        /// </summary>
        public string RedisConnection { get; set; }

        /// <summary>
        /// list of Ips that rate limit will by-pass for them
        /// </summary>
        public List<string> IpWhiteList { get; set; }

    }
}