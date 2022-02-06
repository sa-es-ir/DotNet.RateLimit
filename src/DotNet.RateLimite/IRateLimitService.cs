using System.Threading.Tasks;

namespace DotNet.RateLimit
{
    /// <summary>
    /// rate limit service
    /// </summary>
    public interface IRateLimitService
    {
        /// <summary>
        /// consider request can proceed or not based on key
        /// </summary>
        /// <param name="resourceKey"></param>
        /// <param name="periodInSec"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<bool> HasAccessAsync(string resourceKey, int periodInSec, int limit);
    }
}