using DotNet.RateLimiter.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace DotNet.RateLimiter.Interfaces
{
    public interface IRateLimitCoordinator
    {
        Task<bool> CheckRateLimitAsync(ActionExecutingContext context, RateLimitAttributeParams ratelimitParams);

        Task<bool> CheckRateLimitAsync(EndpointFilterInvocationContext context, RateLimitEndPointParams ratelimitParams);
    }
}