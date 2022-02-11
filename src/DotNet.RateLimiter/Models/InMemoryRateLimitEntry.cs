using System;

namespace DotNet.RateLimiter.Models
{
    public class InMemoryRateLimitEntry
    {
        /// <summary>
        /// consider this entry is expired or not
        /// </summary>
        public DateTime Expiration { get; set; }

        /// <summary>
        /// sum of requests count
        /// </summary>
        public long Total { get; set; }
    }
}