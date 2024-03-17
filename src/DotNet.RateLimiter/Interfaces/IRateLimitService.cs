using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace DotNet.RateLimiter.Interfaces
{
    public interface IRateLimitService
    {
        /// <summary>
        /// consider request can proceed or not based on the given key
        /// </summary>
        /// <param name="resourceKey"></param>
        /// <param name="periodInSec"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<bool> HasAccessAsync(string resourceKey, int periodInSec, int limit);
    }
}