namespace DotNet.RateLimit
{
    public class RateLimitOptions
    {
        /// <summary>
        /// if true will bypass all requests
        /// </summary>
        public bool EnableRateLimit { get; set; }

        /// <summary>
        /// in case of rate limit exceeded this status code will be exposed
        /// </summary>
        public int HttpStatusCode { get; set; } = 429;

        public string ErrorMessage { get; set; } = "Rate limit Exceeded";

    }
}