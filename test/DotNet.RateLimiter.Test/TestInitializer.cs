using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNet.RateLimiter.ActionFilters;
using DotNet.RateLimiter.Interfaces;
using DotNet.RateLimiter.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNet.RateLimiter.Test;

public class TestInitializer
{
    private static readonly Random Random;

    static TestInitializer()
    {
        Random = new Random();
    }

    public static ActionContext SetupActionContext(string ipHeaderName = "X-Forwarded-For",
        string ip = "127.0.0.1",
        string controllerName = "TestController",
        string actionName = "TestAction",
        Dictionary<string, object?>? routeParams = null,
        Dictionary<string, object?>? queryParams = null,
        Dictionary<string, object>? bodyParams = null)
    {
        var httpContext = CreateHttpContext(ipHeaderName, ip, routeParams, queryParams);

        var actionContext = new ActionContext(httpContext,
            new RouteData(),
            new ActionDescriptor()
            {
                RouteValues = new Dictionary<string, string?>()
                {
                    { "Controller", controllerName },
                    { "Action", actionName }
                }
            },
            new ModelStateDictionary());

        return actionContext;
    }

    private static DefaultHttpContext CreateHttpContext(string ipHeaderName, string ip,
        Dictionary<string, object?>? routeParams,
        Dictionary<string, object?>? queryParams)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.TryAdd(ipHeaderName, ip);

        if (routeParams != null && routeParams.Any())
            foreach (var routeParam in routeParams)
                httpContext.GetRouteData().Values.TryAdd(routeParam.Key, routeParam.Value);

        if (queryParams != null && queryParams.Any())
            foreach (var queryParam in queryParams)
            {
                httpContext.Request.QueryString = httpContext.Request.QueryString.Add(queryParam.Key, queryParam.Value?.ToString() ?? "test");
            }

        return httpContext;
    }

    public static Task<ActionExecutedContext> ActionExecutionDelegateNext(ActionContext actionContext)
    {
        var ctx = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), null!);
        return Task.FromResult(ctx);
    }

    public static RateLimitAttribute CreateRateLimitFilter(IServiceScope scopeFactory, int limit, int periodInSec, string? routeParams = null,
        string? queryParams = null, string? bodyParams = null, RateLimitScope scope = RateLimitScope.Action)
    {
        return new RateLimitAttribute(
            scopeFactory.ServiceProvider.GetRequiredService<IOptions<RateLimitOptions>>(),
            scopeFactory.ServiceProvider.GetRequiredService<IRateLimitCoordinator>())
        {
            RateLimitParams = new RateLimitParams
            {
                BodyParams = bodyParams,
                Limit = limit,
                PeriodInSec = periodInSec,
                RouteParams = routeParams,
                QueryParams = queryParams,
                Scope = scope
            }
        };
    }

    public static string GetRandomIpAddress()
    {
        return $"{Random.Next(1, 255)}.{Random.Next(0, 255)}.{Random.Next(0, 255)}.{Random.Next(0, 255)}";
    }

    internal static EndpointFilterInvocationContext CreateEndPointContext(string ipHeaderName = "X-Forwarded-For",
        string ip = "127.0.0.1",
        Dictionary<string, object?>? routeParams = null,
        Dictionary<string, object?>? queryParams = null)
    {
        var httpContext = CreateHttpContext(ipHeaderName, ip, routeParams, queryParams);

        httpContext.SetEndpoint(new Endpoint(null, null, displayName: Guid.NewGuid().ToString()));

        return EndpointFilterInvocationContext.Create(httpContext);
    }
}