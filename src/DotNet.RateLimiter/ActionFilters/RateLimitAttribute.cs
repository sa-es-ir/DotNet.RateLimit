using System;
using System.Linq;
using System.Threading.Tasks;
using DotNet.RateLimiter.Extensions;
using DotNet.RateLimiter.Interfaces;
using DotNet.RateLimiter.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DotNet.RateLimiter.ActionFilters
{
    public class RateLimitAttribute : IAsyncActionFilter, IOrderedFilter
    {
        private readonly ILogger<RateLimitAttribute> _logger;
        private readonly IRateLimitService _rateLimitService;
        private readonly IOptions<RateLimitOptions> _options;

        public RateLimitAttribute(ILogger<RateLimitAttribute> logger, IRateLimitService rateLimitService, IOptions<RateLimitOptions> options)
        {
            _logger = logger;
            _rateLimitService = rateLimitService;
            _options = options;
        }

        public int Order { get; set; }
        public int PeriodInSec { get; set; }
        public int Limit { get; set; }
        public string RouteParams { get; set; }
        public string QueryParams { get; set; }
        public RateLimitScope Scope { get; set; }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                //bypass request if rate limit was disable
                if (!_options.Value.EnableRateLimit)
                {
                    await next.Invoke();
                    return;
                }

                //get current user IP based on header name
                var userIp = context.HttpContext.Request.GetUserIp(_options.Value.IpHeaderName).ToString();

                //skip rate limit if IP is in white list
                if (_options.Value.IpWhiteList.Contains(userIp))
                {
                    await next.Invoke();
                    return;
                }

                var requestKey = userIp;

                if (!string.IsNullOrWhiteSpace(_options.Value.ClientIdentifier) &&
                    context.HttpContext.Request.Headers.TryGetValue(_options.Value.ClientIdentifier, out var clientId))
                {
                    //skip rate limit for client identifiers in white list
                    if (_options.Value.ClientIdentifierWhiteList.Contains(clientId))
                        return;

                    requestKey = clientId.ToString();
                }

                //if action had ignored rate limit then skip it
                var filters = context.Filters;
                if (filters.OfType<IIgnoreRateLimitFilter>().Any())
                {
                    await next.Invoke();
                    return;
                }

                var rateLimitKey = ProvideRateLimitKey(context, requestKey);

                var hasAccess = await _rateLimitService.HasAccessAsync(rateLimitKey, PeriodInSec, Limit);

                if (!hasAccess)
                {
                    context.HttpContext.Response.StatusCode = _options.Value.HttpStatusCode;

                    context.HttpContext.Response.ContentType = "application/json";

                    var response = new RateLimitResponse()
                    {
                        Code = _options.Value.HttpStatusCode,
                        Message = _options.Value.ErrorMessage
                    };

                    await context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(response));
                }
                else
                    await next.Invoke();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, e.Message);
                await next.Invoke();
            }
        }

        private string ProvideRateLimitKey(ActionExecutingContext context, string requestKey)
        {
            //get controller and action name
            var controller = context.ActionDescriptor.RouteValues["Controller"];
            var action = context.ActionDescriptor.RouteValues["Action"];

            var rateLimitKey = $"{requestKey}:{controller}";

            //if scope is action then add action name to the key to consider each action separately
            if (Scope == RateLimitScope.Action)
                rateLimitKey = $"{rateLimitKey}:{action}";


            if (!string.IsNullOrEmpty(RouteParams))
            {
                var parameters = RouteParams.Split(',', StringSplitOptions.RemoveEmptyEntries);

                foreach (var parameter in parameters)
                {
                    if (context.HttpContext.GetRouteData().Values.TryGetValue(parameter, out var routeValue))
                        rateLimitKey += $"{routeValue}:";
                }
            }

            if (!string.IsNullOrEmpty(QueryParams))
            {
                var parameters = QueryParams.Split(',', StringSplitOptions.RemoveEmptyEntries);

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
                                rateLimitKey += $"{items[0]}:";
                                break;
                            default:
                                rateLimitKey += string.Join(":", items);
                                break;
                        }
                    }
                }
            }

            return rateLimitKey;
        }
    }
}