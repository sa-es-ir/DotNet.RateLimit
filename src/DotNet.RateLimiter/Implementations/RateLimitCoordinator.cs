using DotNet.RateLimiter.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using DotNet.RateLimiter.Models;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using DotNet.RateLimiter.ActionFilters;
using System.Text;
using System;
using System.Linq;
using DotNet.RateLimiter.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DotNet.RateLimiter.Implementations;

public class RateLimitCoordinator : IRateLimitCoordinator
{
    private readonly ILogger<RateLimitCoordinator> _logger;
    private readonly IRateLimitService _rateLimitService;
    private readonly IOptions<RateLimitOptions> _options;

    public RateLimitCoordinator(ILogger<RateLimitCoordinator> logger,
        IRateLimitService rateLimitService,
        IOptions<RateLimitOptions> options)
    {
        _logger = logger;
        _rateLimitService = rateLimitService;
        _options = options;
    }


    public async Task<bool> CheckRateLimitAsync(ActionExecutingContext context, RateLimitParams ratelimitParams)
    {
        try
        {
            (bool ShortCircuit, bool HasAccess, string RateLimitPrefixKey) = InitialChecking(context.HttpContext, ratelimitParams);

            if (ShortCircuit)
                return HasAccess;

            //if action had ignored rate limit then skip it
            var filters = context.Filters;
            if (filters.OfType<IIgnoreRateLimitFilter>().Any())
            {
                return true;
            }

            context.ActionDescriptor.RouteValues.TryGetValue("Controller", out var controller);
            context.ActionDescriptor.RouteValues.TryGetValue("Action", out var action);

            var rateLimitKey = new StringBuilder();

            rateLimitKey
                .Append(RateLimitPrefixKey)
                .Append(context.HttpContext.Request.Method)
                .Append(controller);

            //if scope is action then add action name to the key to consider each action separately
            if (ratelimitParams.Scope == RateLimitScope.Action)
                rateLimitKey.Append(action);

            ProvideRateLimitKey(context.HttpContext, ratelimitParams, rateLimitKey);

            SetBodyParamsRateLimitKey(context, ratelimitParams, rateLimitKey);

            return await _rateLimitService.HasAccessAsync(rateLimitKey.ToString(), ratelimitParams.PeriodInSec, ratelimitParams.Limit);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, e.Message);
            return true;
        }
    }

    private (bool ShortCircuit, bool HasAccess, string RateLimitPrefixKey) InitialChecking(HttpContext httpContext, RateLimitParams ratelimitParams)
    {
        //bypass request if rate limit was disable
        if (!_options.Value.EnableRateLimit)
        {
            return (true, true, default);
        }

        if (ratelimitParams.Limit <= 0 || ratelimitParams.PeriodInSec <= 0)
            throw new ArgumentOutOfRangeException("PeriodInSec and Limit must be greater than 0 for RateLimiter");

        //get current user IP based on header name
        var userIp = httpContext.Request.GetUserIp(_options.Value.IpHeaderName)?.ToString();

        //skip rate limit if IP is in white list
        if (_options.Value.IpWhiteList.Contains(userIp))
        {
            return (true, true, default);
        }

        var rateLimitPrefixKey = userIp;

        if (!string.IsNullOrWhiteSpace(_options.Value.ClientIdentifier) &&
            httpContext.Request.Headers.TryGetValue(_options.Value.ClientIdentifier, out var clientId))
        {
            //skip rate limit for client identifiers in white list
            if (_options.Value.ClientIdentifierWhiteList.Contains(clientId))
            {
                return (true, true, default);
            }

            rateLimitPrefixKey = clientId.ToString();
        }

        return (false, true, rateLimitPrefixKey);
    }

#if NET7_0_OR_GREATER
    public async Task<bool> CheckRateLimitAsync(EndpointFilterInvocationContext context, RateLimitEndPointParams ratelimitParams)
    {
        try
        {
            var rateLimitParams = new RateLimitParams
            {
                Limit = ratelimitParams.Limit,
                PeriodInSec = ratelimitParams.PeriodInSec,
                QueryParams = ratelimitParams.QueryParams,
                RouteParams = ratelimitParams.RouteParams
            };

            (bool ShortCircuit, bool HasAccess, string RateLimitPrefixKey) = InitialChecking(context.HttpContext, rateLimitParams);

            if (ShortCircuit)
                return HasAccess;

            var rateLimitKey = new StringBuilder();
            rateLimitKey
                .Append(RateLimitPrefixKey)
                .Append(context.HttpContext.Request.Method)
                .Append(context.HttpContext.GetEndpoint().DisplayName);

            ProvideRateLimitKey(context.HttpContext, rateLimitParams, rateLimitKey);

            return await _rateLimitService.HasAccessAsync(rateLimitKey.ToString(), ratelimitParams.PeriodInSec, ratelimitParams.Limit);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, e.Message);
            return true;
        }
    }
#endif

    private void ProvideRateLimitKey(HttpContext httpContext, RateLimitParams ratelimitParams, StringBuilder rateLimitKey)
    {
        if (!string.IsNullOrWhiteSpace(ratelimitParams.RouteParams))
        {
            var parameters = ratelimitParams.RouteParams.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var parameter in parameters)
            {
                if (httpContext.GetRouteData().Values.TryGetValue(parameter, out var routeValue))
                    rateLimitKey.Append(routeValue);
            }
        }

        if (!string.IsNullOrWhiteSpace(ratelimitParams.QueryParams))
        {
            var parameters = ratelimitParams.QueryParams.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var parameter in parameters)
            {
                if (httpContext.Request.Query.TryGetValue(parameter, out var queryParams))
                {
                    var items = queryParams.ToArray();

                    switch (items.Length)
                    {
                        case 0:
                            continue;
                        case 1:
                            rateLimitKey.Append(items[0]).Append(":");
                            break;
                        default:
                            rateLimitKey.Append(string.Join(":", items));
                            break;
                    }
                }
            }
        }
    }

    private static void SetBodyParamsRateLimitKey(ActionExecutingContext context, RateLimitParams ratelimitParams, StringBuilder rateLimitKey)
    {
        if (!string.IsNullOrWhiteSpace(ratelimitParams.BodyParams))
        {
            var parameters = ratelimitParams.BodyParams.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var jsonSerializer = JsonConvert.SerializeObject(context.ActionArguments);
            var obj = JObject.Parse(jsonSerializer);

            foreach (var parameter in parameters)
            {
                var rootProperty = obj.Root.First().Values().FirstOrDefault(x => x.Type == JTokenType.Property
                         && string.Equals(((JProperty)x).Name, parameter, StringComparison.OrdinalIgnoreCase));

                if (rootProperty is JProperty property)
                {
                    rateLimitKey.Append(property.Value).Append(":");
                }
            }
        }
    }
}
