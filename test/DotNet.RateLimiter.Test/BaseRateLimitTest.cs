using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DotNet.RateLimiter.Test;

public class BaseRateLimitTest
{
    private readonly IServiceScopeFactory _scopeFactory;

    public BaseRateLimitTest(IServiceScopeFactory scopeFactory)
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

    [Theory]
    [MemberData(nameof(TestDataProvider.OkTestDataWithNoParams), MemberType = typeof(TestDataProvider))]
    public async Task ExtraRequest_WhiteListIp_Returns_OK(int limit, int periodInSec, Dictionary<string, object?> actionArguments, HttpStatusCode expectedResult)
    {
        using var scope = _scopeFactory.CreateScope();
        var rateLimitAction = TestInitializer.CreateRateLimitFilter(scope, limit, periodInSec);

        var actionContext = TestInitializer.SetupActionContext(ip: "8.8.8.8");//same ip in white list ip in appsettings.json

        var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), actionArguments, null!);

        //fist is ok
        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, () => TestInitializer.ActionExecutionDelegateNext(actionContext));

        //second request should be banned but in case of white list ip it's ok
        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, () => TestInitializer.ActionExecutionDelegateNext(actionContext));

        actionExecutingContext.HttpContext.Response.StatusCode.Should().Be((int)expectedResult);
    }

    [Theory]
    [MemberData(nameof(TestDataProvider.OkTestDataWithRouteParams), MemberType = typeof(TestDataProvider))]
    public async Task NormalRequest_WithRouteParam_Returns_OK(int limit, int periodInSec, Dictionary<string, object?> actionArguments, HttpStatusCode expectedResult)
    {
        using var scope = _scopeFactory.CreateScope();
        var rateLimitAction = TestInitializer.CreateRateLimitFilter(scope, limit, periodInSec, routeParams: string.Join(",", actionArguments.Select(x => x.Key)));

        var actionContext = TestInitializer.SetupActionContext(ip: TestInitializer.GetRandomIpAddress(), routeParams: actionArguments);

        var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), actionArguments, null!);

        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, () => TestInitializer.ActionExecutionDelegateNext(actionContext));

        actionExecutingContext.HttpContext.Response.StatusCode.Should().Be((int)expectedResult);
    }

    [Theory]
    [MemberData(nameof(TestDataProvider.TooManyRequestTestDataWithRouteParams), MemberType = typeof(TestDataProvider))]
    public async Task ExtraRequest_WithRouteParam_Returns_TooManyRequest(int limit, int periodInSec, Dictionary<string, object?> actionArguments, HttpStatusCode expectedResult)
    {
        using var scope = _scopeFactory.CreateScope();
        var rateLimitAction = TestInitializer.CreateRateLimitFilter(scope, limit, periodInSec, routeParams: string.Join(",", actionArguments.Select(x => x.Key)));

        var actionContext = TestInitializer.SetupActionContext(ip: TestInitializer.GetRandomIpAddress(), routeParams: actionArguments);

        var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), actionArguments, null!);

        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, () => TestInitializer.ActionExecutionDelegateNext(actionContext));

        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, () => TestInitializer.ActionExecutionDelegateNext(actionContext));

        actionExecutingContext.HttpContext.Response.StatusCode.Should().Be((int)expectedResult);
    }

    [Theory]
    [MemberData(nameof(TestDataProvider.OkTestDataWithRouteAndQueryParams), MemberType = typeof(TestDataProvider))]
    public async Task NormalRequest_WithRouteParam_QueryParam_Returns_OK(int limit, int periodInSec, Dictionary<string, object?> actionArguments, Dictionary<string, object> queryParams, HttpStatusCode expectedResult)
    {
        using var scope = _scopeFactory.CreateScope();
        var rateLimitAction = TestInitializer.CreateRateLimitFilter(scope, limit, periodInSec,
            string.Join(",", actionArguments.Select(x => x.Key)),
            string.Join(",", queryParams.Select(x => x.Key)));

        var actionContext = TestInitializer.SetupActionContext(ip: TestInitializer.GetRandomIpAddress(), routeParams: actionArguments, queryParams: queryParams);

        var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), actionArguments, null!);

        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, () => TestInitializer.ActionExecutionDelegateNext(actionContext));

        actionExecutingContext.HttpContext.Response.StatusCode.Should().Be((int)expectedResult);
    }

    [Theory]
    [MemberData(nameof(TestDataProvider.TooManyRequestTestDataWithRouteAndQueryParams), MemberType = typeof(TestDataProvider))]
    public async Task ExtraRequest_WithRouteParam_QueryParam_Returns_TooManyRequest(int limit, int periodInSec, Dictionary<string, object?> actionArguments, Dictionary<string, object> queryParams, HttpStatusCode expectedResult)
    {
        using var scope = _scopeFactory.CreateScope();
        var rateLimitAction = TestInitializer.CreateRateLimitFilter(scope, limit, periodInSec, routeParams: string.Join(",", actionArguments.Select(x => x.Key)));

        var actionContext = TestInitializer.SetupActionContext(ip: TestInitializer.GetRandomIpAddress(), routeParams: actionArguments, queryParams: queryParams);

        var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), actionArguments, null!);

        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, () => TestInitializer.ActionExecutionDelegateNext(actionContext));

        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, () => TestInitializer.ActionExecutionDelegateNext(actionContext));

        actionExecutingContext.HttpContext.Response.StatusCode.Should().Be((int)expectedResult);
    }

    [Theory]
    [MemberData(nameof(TestDataProvider.TooManyRequestTestDataWithBodyParams), MemberType = typeof(TestDataProvider))]
    public async Task ExtraRequest_WithBodyParam_Returns_TooManyRequest(int limit, int periodInSec,
        Dictionary<string, object?> actionArguments, HttpStatusCode expectedResult)
    {

        using var scope = _scopeFactory.CreateScope();
        var bodyParams = "id,name";
        var rateLimitAction = TestInitializer.CreateRateLimitFilter(scope, limit, periodInSec, bodyParams);

        var actionContext = TestInitializer.SetupActionContext(ip: TestInitializer.GetRandomIpAddress(), bodyParams: actionArguments!);

        var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), actionArguments, null!);

        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, () => TestInitializer.ActionExecutionDelegateNext(actionContext));

        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, () => TestInitializer.ActionExecutionDelegateNext(actionContext));

        actionExecutingContext.HttpContext.Response.StatusCode.Should().Be((int)expectedResult);
    }

    [Theory]
    [MemberData(nameof(TestDataProvider.OkTestDataWithBodyParams), MemberType = typeof(TestDataProvider))]
    public async Task NormalRequest_WithBodyParam_Returns_OK(int limit, int periodInSec, Dictionary<string, object?> actionArguments, HttpStatusCode expectedResult)
    {
        using var scope = _scopeFactory.CreateScope();
        var bodyParams = "id,name";
        var rateLimitAction = TestInitializer.CreateRateLimitFilter(scope, limit, periodInSec, bodyParams);

        var actionContext = TestInitializer.SetupActionContext(ip: TestInitializer.GetRandomIpAddress(), bodyParams: actionArguments!);

        var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), actionArguments, null!);

        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, () => TestInitializer.ActionExecutionDelegateNext(actionContext));

        actionExecutingContext.HttpContext.Response.StatusCode.Should().Be((int)expectedResult);
    }
}