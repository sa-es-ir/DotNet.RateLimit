namespace DotNet.RateLimiter.Models
{
    public class RateLimitOptions
    {
        /// <summary>
        /// if false will bypass all requests
        /// </summary>
        public bool EnableRateLimit { get; set; } = true;

        /// <summary>
        /// in case of rate limit exceeded this status code will be exposed
        /// </summary>
        public int HttpStatusCode { get; set; } = 429;

        /// <summary>
        /// when rate limit exceeded this message will return
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

    }
}