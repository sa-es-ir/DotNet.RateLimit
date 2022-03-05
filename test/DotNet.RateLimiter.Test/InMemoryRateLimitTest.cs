using System;
using System.Collections.Generic;
using System.Net;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.DependencyInjection;

namespace DotNet.RateLimiter.Test;

[Startup(typeof(StartupRedis))]
public class InMemoryRateLimitTest
{
    private readonly IServiceScopeFactory _scopeFactory;

    public InMemoryRateLimitTest(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    [Theory]
    [MemberData(nameof(TestDataProvider.OkTestDataWithNoParams), MemberType = typeof(TestDataProvider))]
    public async Task NormalRequest_Returns_Ok(int limit, int periodInSec, Dictionary<string, object?> actionArguments, HttpStatusCode expectedResult)
    {
        using var scope = _scopeFactory.CreateScope();
        var rateLimitAction = TestInitializer.CreateRateLimitFilter(scope, limit, periodInSec);

        var actionContext = TestInitializer.SetupActionContext(ip: TestInitializer.GetRandomIpAddress());

        var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), actionArguments, null!);
        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, () => TestInitializer.ActionExecutionDelegateNext(actionContext));

        actionExecutingContext.HttpContext.Response.StatusCode.Should().Be((int)expectedResult);
    }

    [Theory]
    [MemberData(nameof(TestDataProvider.TooManyRequestTestDataWithNoParams), MemberType = typeof(TestDataProvider))]
    public async Task ExtraRequest_Returns_TooManyRequest(int limit, int periodInSec, Dictionary<string, object?> actionArguments, HttpStatusCode expectedResult)
    {
        using var scope = _scopeFactory.CreateScope();
        var rateLimitAction = TestInitializer.CreateRateLimitFilter(scope, limit, periodInSec);

        var actionContext = TestInitializer.SetupActionContext(ip: TestInitializer.GetRandomIpAddress());

        var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), actionArguments, null!);

        //fist is ok
        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, () => TestInitializer.ActionExecutionDelegateNext(actionContext));

        //second request should be banned
        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, () => TestInitializer.ActionExecutionDelegateNext(actionContext));

        actionExecutingContext.HttpContext.Response.StatusCode.Should().Be((int)expectedResult);
    }

    [Theory]
    [MemberData(nameof(TestDataProvider.OkTestDataWithNoParams), MemberType = typeof(TestDataProvider))]
    public async Task ExtraRequest_DifferentIp_Returns_Ok(int limit, int periodInSec, Dictionary<string, object?> actionArguments, HttpStatusCode expectedResult)
    {
        using var scope = _scopeFactory.CreateScope();
        var rateLimitAction = TestInitializer.CreateRateLimitFilter(scope, limit, periodInSec);

        var actionContext = TestInitializer.SetupActionContext(ip: TestInitializer.GetRandomIpAddress());

        var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), actionArguments, null!);
        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, () => TestInitializer.ActionExecutionDelegateNext(actionContext));

        actionExecutingContext.HttpContext.Response.StatusCode.Should().Be((int)expectedResult);

        actionContext = TestInitializer.SetupActionContext(ip: TestInitializer.GetRandomIpAddress());

        actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), actionArguments, null!);
        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, () => TestInitializer.ActionExecutionDelegateNext(actionContext));

        actionExecutingContext.HttpContext.Response.StatusCode.Should().Be((int)expectedResult);
    }
}