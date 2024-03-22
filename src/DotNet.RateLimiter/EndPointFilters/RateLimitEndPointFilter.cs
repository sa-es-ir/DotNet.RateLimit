using DotNet.RateLimiter.Interfaces;
using DotNet.RateLimiter.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace DotNet.RateLimiter.EndPointFilters;

#if NET7_0_OR_GREATER
public class RateLimitEndPointFilter : IEndpointFilter
{
    private readonly IOptions<RateLimitOptions> _options;
    private readonly IRateLimitCoordinator _rateLimitCoordinator;

    public RateLimitEndPointFilter(IOptions<RateLimitOptions> options,
        IRateLimitCoordinator rateLimitCoordinator)
    {
        _options = options;
        _rateLimitCoordinator = rateLimitCoordinator;
    }

    public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var rateLimitParams = context.HttpContext.Features.Get<RateLimitEndPointParams>();

        ArgumentNullException.ThrowIfNull(nameof(rateLimitParams));

        var goodToGo = await _rateLimitCoordinator.CheckRateLimitAsync(context, rateLimitParams);

        if (goodToGo)
            return await next(context);
        else
        {
            var response = new RateLimitResponse()
            {
                Code = _options.Value.HttpStatusCode,
                Message = _options.Value.ErrorMessage
            };

            return Results.Json(response, contentType: "application/json", statusCode: _options.Value.HttpStatusCode);
        }
    }
}

#endif