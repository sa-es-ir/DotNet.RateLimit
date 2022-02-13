using System.Collections.Generic;

namespace DotNet.RateLimiter.Models
{
    public class RateLimitOptions
    {
        public RateLimitOptions()
        {
            IpWhiteList = new List<string>();
        }
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

        /// <summary>
        /// consider how retrieve IP address from request header default is X-Forwarded-For.
        /// </summary>
        public string IpHeaderName { get; set; } = "X-Forwarded-For";

        /// <summary>
        /// by default rate limit work on IP address but if userIdentifier set it will look at request header to get client identifier
        /// </summary>
        public string ClientIdentifier { get; set; }

        /// <summary>
        /// list of client identifiers that rate limit will by-pass for them
        /// </summary>
        public List<string> ClientIdentifierWhiteList { get; set; }

    }
}