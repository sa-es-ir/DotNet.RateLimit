using System.Net;

namespace DotNet.RateLimiter.Models
{
    public class RateLimitResponse
    {
        public string Message { get; set; }

        public int Code { get; set; }

        public string Status => ((HttpStatusCode)Code).ToString();
    }
}