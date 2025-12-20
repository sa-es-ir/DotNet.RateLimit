using DotNet.RateLimiter.Interfaces;
using DotNet.RateLimiter.Models;
using DotNet.RateLimiter.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace DotNet.RateLimiter.EndPointFilters;

#if NET8_0_OR_GREATER
public class RateLimitEndPointFilter : IEndpointFilter
{
    private readonly IOptions<RateLimitOptions> _options;
    private readonly IRateLimitCoordinator _rateLimitCoordinator;

    public RateLimitEndPointFilter(
        IOptions<RateLimitOptions> options,
        IRateLimitCoordinator rateLimitCoordinator)
    {
        _options = options;
        _rateLimitCoordinator = rateLimitCoordinator;
    }

    public async ValueTask<object> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var rateLimitParams = context.HttpContext.Features.Get<RateLimitEndPointParams>();

        if (rateLimitParams == null)
        {
            throw new ArgumentNullException(nameof(rateLimitParams));
        }

        var goodToGo = await _rateLimitCoordinator.CheckRateLimitAsync(context, rateLimitParams);

        if (goodToGo)
            return await next(context);

        var responseBody = RateLimitResponseBuilder.BuildResponse(_options.Value);

        // Parse the JSON string to a JSON node for Results.Json
        JsonNode responseObject = JsonNode.Parse(responseBody);

        return Results.Json(
            responseObject,
            contentType: "application/json",
            statusCode: _options.Value.HttpStatusCode);
    }
}
#endif