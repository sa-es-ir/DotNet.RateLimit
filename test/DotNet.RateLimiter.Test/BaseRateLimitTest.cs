using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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

        actionExecutingContext.HttpContext.Response.StatusCode.ShouldBe((int)expectedResult);
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

        actionExecutingContext.HttpContext.Response.StatusCode.ShouldBe((int)expectedResult);
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

        actionExecutingContext.HttpContext.Response.StatusCode.ShouldBe((int)expectedResult);

        actionContext = TestInitializer.SetupActionContext(ip: TestInitializer.GetRandomIpAddress());

        actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), actionArguments, null!);
        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, () => TestInitializer.ActionExecutionDelegateNext(actionContext));

        actionExecutingContext.HttpContext.Response.StatusCode.ShouldBe((int)expectedResult);
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

        actionExecutingContext.HttpContext.Response.StatusCode.ShouldBe((int)expectedResult);
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

        actionExecutingContext.HttpContext.Response.StatusCode.ShouldBe((int)expectedResult);
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

        actionExecutingContext.HttpContext.Response.StatusCode.ShouldBe((int)expectedResult);
    }

    [Theory]
    [MemberData(nameof(TestDataProvider.OkTestDataWithRouteAndQueryParams), MemberType = typeof(TestDataProvider))]
    public async Task NormalRequest_WithRouteParam_QueryParam_Returns_OK(int limit, int periodInSec, Dictionary<string, object?> actionArguments, Dictionary<string, object> queryParams, HttpStatusCode expectedResult)
    {
        using var scope = _scopeFactory.CreateScope();
        var rateLimitAction = TestInitializer.CreateRateLimitFilter(scope, limit, periodInSec,
            string.Join(",", actionArguments.Select(x => x.Key)),
            string.Join(",", queryParams.Select(x => x.Key)));

        var actionContext = TestInitializer.SetupActionContext(ip: TestInitializer.GetRandomIpAddress(), routeParams: actionArguments, queryParams: queryParams!);

        var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), actionArguments, null!);

        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, () => TestInitializer.ActionExecutionDelegateNext(actionContext));

        actionExecutingContext.HttpContext.Response.StatusCode.ShouldBe((int)expectedResult);
    }

    [Theory]
    [MemberData(nameof(TestDataProvider.TooManyRequestTestDataWithRouteAndQueryParams), MemberType = typeof(TestDataProvider))]
    public async Task ExtraRequest_WithRouteParam_QueryParam_Returns_TooManyRequest(int limit, int periodInSec, Dictionary<string, object?> actionArguments, Dictionary<string, object> queryParams, HttpStatusCode expectedResult)
    {
        using var scope = _scopeFactory.CreateScope();
        var rateLimitAction = TestInitializer.CreateRateLimitFilter(scope, limit, periodInSec, routeParams: string.Join(",", actionArguments.Select(x => x.Key)));

        var actionContext = TestInitializer.SetupActionContext(ip: TestInitializer.GetRandomIpAddress(), routeParams: actionArguments, queryParams: queryParams!);

        var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), actionArguments, null!);

        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, () => TestInitializer.ActionExecutionDelegateNext(actionContext));

        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, () => TestInitializer.ActionExecutionDelegateNext(actionContext));

        actionExecutingContext.HttpContext.Response.StatusCode.ShouldBe((int)expectedResult);
    }

    [Theory]
    [MemberData(nameof(TestDataProvider.TooManyRequestTestDataWithBodyParams), MemberType = typeof(TestDataProvider))]
    public async Task ExtraRequest_WithBodyParam_Returns_TooManyRequest(int limit, int periodInSec,
        Dictionary<string, object?> actionArguments, HttpStatusCode expectedResult)
    {

        using var scope = _scopeFactory.CreateScope();
        var bodyParams = "id,name";
        var rateLimitAction = TestInitializer.CreateRateLimitFilter(scope, limit, periodInSec, bodyParams: bodyParams);

        var actionContext = TestInitializer.SetupActionContext(ip: TestInitializer.GetRandomIpAddress(), bodyParams: actionArguments!);

        var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), actionArguments, null!);

        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, () => TestInitializer.ActionExecutionDelegateNext(actionContext));

        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, () => TestInitializer.ActionExecutionDelegateNext(actionContext));

        actionExecutingContext.HttpContext.Response.StatusCode.ShouldBe((int)expectedResult);
    }

    [Theory]
    [MemberData(nameof(TestDataProvider.OkTestDataWithBodyParams), MemberType = typeof(TestDataProvider))]
    public async Task NormalRequest_WithBodyParam_Returns_OK(int limit, int periodInSec, Dictionary<string, object?> actionArguments, HttpStatusCode expectedResult)
    {
        using var scope = _scopeFactory.CreateScope();
        var bodyParams = "id,name";
        var rateLimitAction = TestInitializer.CreateRateLimitFilter(scope, limit, periodInSec, bodyParams: bodyParams);

        var actionContext = TestInitializer.SetupActionContext(ip: TestInitializer.GetRandomIpAddress(), bodyParams: actionArguments!);

        var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), actionArguments, null!);

        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, () => TestInitializer.ActionExecutionDelegateNext(actionContext));

        actionExecutingContext.HttpContext.Response.StatusCode.ShouldBe((int)expectedResult);
    }

    // issue #53: CancellationToken in ActionArguments made serialization throw, so the limiter failed open
    [Fact]
    public async Task ExtraRequest_WithBodyParam_And_CancellationToken_Returns_TooManyRequest()
    {
        var arguments = new Dictionary<string, object?>
        {
            { "request", new { NationalCode = "123" } },
            { "cancellationToken", new CancellationTokenSource().Token }
        };

        var statusCode = await ExecuteRequestsAsync("NationalCode", null, arguments, arguments);

        statusCode.ShouldBe((int)HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task ExtraRequest_WithCancellationTokenBeforeBody_Returns_TooManyRequest()
    {
        var arguments = new Dictionary<string, object?>
        {
            { "cancellationToken", new CancellationTokenSource().Token },
            { "request", new { NationalCode = "123" } }
        };

        var statusCode = await ExecuteRequestsAsync("NationalCode", null, arguments, arguments);

        statusCode.ShouldBe((int)HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task ExtraRequest_WithCancellationToken_DifferentBodyParam_Returns_OK()
    {
        var token = new CancellationTokenSource().Token;

        var statusCode = await ExecuteRequestsAsync("NationalCode", null,
            new Dictionary<string, object?> { { "request", new { NationalCode = "123" } }, { "cancellationToken", token } },
            new Dictionary<string, object?> { { "request", new { NationalCode = "456" } }, { "cancellationToken", token } });

        statusCode.ShouldBe((int)HttpStatusCode.OK);
    }

    [Fact]
    public async Task ExtraRequest_WithFromBodyNotFirstArgument_DifferentBodyParam_Returns_OK()
    {
        var parameters = new List<ParameterDescriptor>
        {
            new() { Name = "id", BindingInfo = new BindingInfo { BindingSource = BindingSource.Path } },
            new() { Name = "request", BindingInfo = new BindingInfo { BindingSource = BindingSource.Body } }
        };

        var statusCode = await ExecuteRequestsAsync("NationalCode", parameters,
            new Dictionary<string, object?> { { "id", 1 }, { "request", new { NationalCode = "123" } } },
            new Dictionary<string, object?> { { "id", 1 }, { "request", new { NationalCode = "456" } } });

        statusCode.ShouldBe((int)HttpStatusCode.OK);
    }

    // sends each request from the same IP to a Limit = 1 action and returns the last response status code
    private async Task<int> ExecuteRequestsAsync(string bodyParams, IList<ParameterDescriptor>? parameters, params Dictionary<string, object?>[] requestsArguments)
    {
        using var scope = _scopeFactory.CreateScope();
        var rateLimitAction = TestInitializer.CreateRateLimitFilter(scope, limit: 1, periodInSec: 60, bodyParams: bodyParams);
        var ip = TestInitializer.GetRandomIpAddress();
        var statusCode = 0;

        foreach (var arguments in requestsArguments)
        {
            var actionContext = TestInitializer.SetupActionContext(ip: ip);
            if (parameters != null)
                actionContext.ActionDescriptor.Parameters = parameters;

            var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), arguments, null!);
            await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, () => TestInitializer.ActionExecutionDelegateNext(actionContext));

            statusCode = actionExecutingContext.HttpContext.Response.StatusCode;
        }

        return statusCode;
    }
}