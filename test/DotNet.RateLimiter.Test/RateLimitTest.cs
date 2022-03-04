using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet.RateLimiter.ActionFilters;
using DotNet.RateLimiter.Interfaces;
using DotNet.RateLimiter.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace DotNet.RateLimiter.Test;

public class RateLimitTest
{
    private readonly IServiceScopeFactory _scopeFactory;

    public RateLimitTest(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    [Fact]
    public async Task FistTest()
    {
        using var scope = _scopeFactory.CreateScope();
        var rateLimitAction = new RateLimitAttribute(
            scope.ServiceProvider.GetRequiredService<ILogger<RateLimitAttribute>>(),
            scope.ServiceProvider.GetRequiredService<IRateLimitService>(),
            scope.ServiceProvider.GetRequiredService<IOptions<RateLimitOptions>>())
        {
            Limit = 1,
            PeriodInSec = 60
        };

        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext,
            new RouteData(),
            new ActionDescriptor()
            {
                RouteValues = new Dictionary<string, string?>()
            {
                {"Controller", "TestController"},
                {"Action", "TestAction"}
            }
            },
            new ModelStateDictionary());

        Task<ActionExecutedContext> Next()
        {
            var ctx = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), null!);
            return Task.FromResult(ctx);
        }

        var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>() { { "id", "1" } }, null!);
        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, Next);

        actionExecutingContext.HttpContext.Response.StatusCode.Should().Be(200);

        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, Next);

        actionExecutingContext.HttpContext.Response.StatusCode.Should().Be(429);
    }
}