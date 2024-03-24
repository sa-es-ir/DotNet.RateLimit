using DotNet.RateLimiter.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace DotNet.RateLimiter.Interfaces
{
    public interface IRateLimitCoordinator
    {
        Task<bool> CheckRateLimitAsync(ActionExecutingContext context, RateLimitParams ratelimitParams);

#if NET7_0_OR_GREATER
        Task<bool> CheckRateLimitAsync(EndpointFilterInvocationContext context, RateLimitEndPointParams ratelimitParams);
#endif
    }
}