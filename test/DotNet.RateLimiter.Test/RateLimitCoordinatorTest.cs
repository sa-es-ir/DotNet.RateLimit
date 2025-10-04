using DotNet.RateLimiter.Interfaces;
using Shouldly;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Xunit.DependencyInjection;

namespace DotNet.RateLimiter.Test;

[Startup(typeof(Startup))]
public class RateLimitCoordinatorTest
{
    private readonly IServiceScopeFactory _scopeFactory;

    public RateLimitCoordinatorTest(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    [Theory]
    [MemberData(nameof(TestDataProvider.TrueTestDataWithNoParams), MemberType = typeof(TestDataProvider))]
    public async Task NormalRequest_WithNoParams_Returns_True(int limit, int periodInSec, bool expectedResult)
    {
        using var scope = _scopeFactory.CreateScope();

        var rateLimitCoordinator = scope.ServiceProvider.GetRequiredService<IRateLimitCoordinator>();

        var endpointContext = TestInitializer.CreateEndPointContext();

        var result = await rateLimitCoordinator.CheckRateLimitAsync(endpointContext, new Models.RateLimitEndPointParams
        {
            Limit = limit,
            PeriodInSec = periodInSec
        });

        result.ShouldBe(expectedResult);
    }

    [Theory]
    [MemberData(nameof(TestDataProvider.FalseTestDataWithNoParams), MemberType = typeof(TestDataProvider))]
    public async Task ManyRequest_WithNoParams_Returns_False(int limit, int periodInSec, bool expectedResult)
    {
        using var scope = _scopeFactory.CreateScope();

        var rateLimitCoordinator = scope.ServiceProvider.GetRequiredService<IRateLimitCoordinator>();

        var endpointContext = TestInitializer.CreateEndPointContext();

        var result = await rateLimitCoordinator.CheckRateLimitAsync(endpointContext, new Models.RateLimitEndPointParams
        {
            Limit = limit,
            PeriodInSec = periodInSec
        });

        result = await rateLimitCoordinator.CheckRateLimitAsync(endpointContext, new Models.RateLimitEndPointParams
        {
            Limit = limit,
            PeriodInSec = periodInSec
        });

        result.ShouldBe(expectedResult);
    }

    [Theory]
    [MemberData(nameof(TestDataProvider.TrueTestDataWithRouteParams), MemberType = typeof(TestDataProvider))]
    public async Task NormalRequest_WithRouteParam_Returns_True(int limit, int periodInSec, Dictionary<string, object?> routeParams, bool expectedResult)
    {
        using var scope = _scopeFactory.CreateScope();

        var rateLimitCoordinator = scope.ServiceProvider.GetRequiredService<IRateLimitCoordinator>();

        var endpointContext = TestInitializer.CreateEndPointContext(routeParams: routeParams);

        var result = await rateLimitCoordinator.CheckRateLimitAsync(endpointContext, new Models.RateLimitEndPointParams
        {
            Limit = limit,
            PeriodInSec = periodInSec,
            RouteParams = string.Join(',', routeParams.Select(x=>x.Key))
        });

        result.ShouldBe(expectedResult);
    }

    [Theory]
    [MemberData(nameof(TestDataProvider.FalseTestDataWithRouteParams), MemberType = typeof(TestDataProvider))]
    public async Task ManyRequest_WithRouteParam_Returns_False(int limit, int periodInSec, Dictionary<string, object?> routeParams, bool expectedResult)
    {
        using var scope = _scopeFactory.CreateScope();

        var rateLimitCoordinator = scope.ServiceProvider.GetRequiredService<IRateLimitCoordinator>();

        var endpointContext = TestInitializer.CreateEndPointContext(routeParams: routeParams);

        var result = await rateLimitCoordinator.CheckRateLimitAsync(endpointContext, new Models.RateLimitEndPointParams
        {
            Limit = limit,
            PeriodInSec = periodInSec,
            RouteParams = string.Join(',', routeParams.Select(x => x.Key))
        });

        result = await rateLimitCoordinator.CheckRateLimitAsync(endpointContext, new Models.RateLimitEndPointParams
        {
            Limit = limit,
            PeriodInSec = periodInSec,
            RouteParams = string.Join(',', routeParams.Select(x => x.Key))
        });

        result.ShouldBe(expectedResult);
    }

    [Theory]
    [MemberData(nameof(TestDataProvider.TrueTestDataWithRouteAndQueryParams), MemberType = typeof(TestDataProvider))]
    public async Task NormalRequest_WithRouteParam_QueryParam_Returns_True(int limit, int periodInSec, 
        Dictionary<string, object?> routeParams, Dictionary<string, object?> queryParams, bool expectedResult)
    {
        using var scope = _scopeFactory.CreateScope();

        var rateLimitCoordinator = scope.ServiceProvider.GetRequiredService<IRateLimitCoordinator>();

        var endpointContext = TestInitializer.CreateEndPointContext(routeParams: routeParams, queryParams: queryParams);

        var result = await rateLimitCoordinator.CheckRateLimitAsync(endpointContext, new Models.RateLimitEndPointParams
        {
            Limit = limit,
            PeriodInSec = periodInSec,
            RouteParams = string.Join(',', routeParams.Select(x => x.Key)),
            QueryParams = string.Join(',', queryParams.Select(x=>x.Key))
        });

        result.ShouldBe(expectedResult);
    }

    [Theory]
    [MemberData(nameof(TestDataProvider.FalseTestDataWithRouteAndQueryParams), MemberType = typeof(TestDataProvider))]
    public async Task ManyRequest_WithRouteParam_QueryParam_Returns_False(int limit, int periodInSec,
        Dictionary<string, object?> routeParams, Dictionary<string, object?> queryParams, bool expectedResult)
    {
        using var scope = _scopeFactory.CreateScope();

        var rateLimitCoordinator = scope.ServiceProvider.GetRequiredService<IRateLimitCoordinator>();

        var endpointContext = TestInitializer.CreateEndPointContext(routeParams: routeParams, queryParams: queryParams);

        var result = await rateLimitCoordinator.CheckRateLimitAsync(endpointContext, new Models.RateLimitEndPointParams
        {
            Limit = limit,
            PeriodInSec = periodInSec,
            RouteParams = string.Join(',', routeParams.Select(x => x.Key)),
            QueryParams = string.Join(',', queryParams.Select(x => x.Key))
        });

        result = await rateLimitCoordinator.CheckRateLimitAsync(endpointContext, new Models.RateLimitEndPointParams
        {
            Limit = limit,
            PeriodInSec = periodInSec,
            RouteParams = string.Join(',', routeParams.Select(x => x.Key)),
            QueryParams = string.Join(',', queryParams.Select(x => x.Key))
        });

        result.ShouldBe(expectedResult);
    }
}
