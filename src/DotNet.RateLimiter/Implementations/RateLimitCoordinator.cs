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


    public async Task<bool> CheckRateLimitAsync(ActionExecutingContext context, RateLimitAttributeParams ratelimitParams)
    {
        try
        {
            //bypass request if rate limit was disable
            if (!_options.Value.EnableRateLimit)
            {
                return true;
            }

            if (ratelimitParams.Limit <= 0)
                return false;

            //get current user IP based on header name
            var userIp = context.HttpContext.Request.GetUserIp(_options.Value.IpHeaderName)?.ToString();

            //skip rate limit if IP is in white list
            if (_options.Value.IpWhiteList.Contains(userIp))
            {
                return true;
            }

            var requestKey = userIp;

            if (!string.IsNullOrWhiteSpace(_options.Value.ClientIdentifier) &&
                context.HttpContext.Request.Headers.TryGetValue(_options.Value.ClientIdentifier, out var clientId))
            {
                //skip rate limit for client identifiers in white list
                if (_options.Value.ClientIdentifierWhiteList.Contains(clientId))
                {
                    return true;
                }

                requestKey = clientId.ToString();
            }

            //if action had ignored rate limit then skip it
            var filters = context.Filters;
            if (filters.OfType<IIgnoreRateLimitFilter>().Any())
            {
                return true;
            }

            var rateLimitKey = ProvideRateLimitKey(context, ratelimitParams, requestKey);

            return await _rateLimitService.HasAccessAsync(rateLimitKey, ratelimitParams.PeriodInSec, ratelimitParams.Limit);


        }
        catch (Exception e)
        {
            _logger.LogCritical(e, e.Message);
            return true;
        }
    }

    public Task<bool> CheckRateLimitAsync(EndpointFilterInvocationContext context, RateLimitEndPointParams ratelimitParams)
    {
        return Task.FromResult(true);
    }

    private string ProvideRateLimitKey(ActionExecutingContext context, RateLimitAttributeParams ratelimitParams, string requestKey)
    {
        context.ActionDescriptor.RouteValues.TryGetValue("Controller", out var controller);
        context.ActionDescriptor.RouteValues.TryGetValue("Action", out var action);

        var rateLimitKey = new StringBuilder();

        rateLimitKey.Append(requestKey).Append(":").Append(controller);

        //if scope is action then add action name to the key to consider each action separately
        if (ratelimitParams.Scope == RateLimitScope.Action)
            rateLimitKey.Append(action);

        if (!string.IsNullOrWhiteSpace(ratelimitParams.RouteParams))
        {
            var parameters = ratelimitParams.RouteParams.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var parameter in parameters)
            {
                if (context.HttpContext.GetRouteData().Values.TryGetValue(parameter, out var routeValue))
                    rateLimitKey.Append(routeValue);
            }
        }

        if (!string.IsNullOrWhiteSpace(ratelimitParams.QueryParams))
        {
            var parameters = ratelimitParams.QueryParams.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var parameter in parameters)
            {
                if (context.HttpContext.Request.Query.TryGetValue(parameter, out _))
                {
                    var items = context.HttpContext.Request.Query[parameter].ToArray();

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

        return rateLimitKey.ToString();
    }
}
