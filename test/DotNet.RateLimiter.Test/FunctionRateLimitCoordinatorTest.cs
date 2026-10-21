using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet.RateLimiter.Interfaces;
using DotNet.RateLimiter.Models;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using Xunit.DependencyInjection;

namespace DotNet.RateLimiter.Test;

[Startup(typeof(Startup))]
public class FunctionRateLimitCoordinatorTest
{
    private readonly IServiceScopeFactory _scopeFactory;

    public FunctionRateLimitCoordinatorTest(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    [Fact]
    public async Task UnderLimit_Returns_True()
    {
        using var scope = _scopeFactory.CreateScope();
        var coordinator = scope.ServiceProvider.GetRequiredService<IRateLimitCoordinator>();

        var result = await coordinator.CheckRateLimitAsync(
            CreateContext(TestInitializer.GetRandomIpAddress()),
            new RateLimitEndPointParams { Limit = 10, PeriodInSec = 60 });

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task OverLimit_Returns_False()
    {
        using var scope = _scopeFactory.CreateScope();
        var coordinator = scope.ServiceProvider.GetRequiredService<IRateLimitCoordinator>();
        var context = CreateContext(TestInitializer.GetRandomIpAddress());
        var parameters = new RateLimitEndPointParams { Limit = 1, PeriodInSec = 60 };

        (await coordinator.CheckRateLimitAsync(context, parameters)).ShouldBeTrue();
        (await coordinator.CheckRateLimitAsync(context, parameters)).ShouldBeFalse();
    }

    [Fact]
    public async Task DistinctFunctionNames_AreCountedIndependently()
    {
        using var scope = _scopeFactory.CreateScope();
        var coordinator = scope.ServiceProvider.GetRequiredService<IRateLimitCoordinator>();
        var ip = TestInitializer.GetRandomIpAddress();
        var parameters = new RateLimitEndPointParams { Limit = 1, PeriodInSec = 60 };
        var firstFunction = CreateContext(ip, functionName: "FirstFunction");
        var secondFunction = CreateContext(ip, functionName: "SecondFunction");

        (await coordinator.CheckRateLimitAsync(firstFunction, parameters)).ShouldBeTrue();
        (await coordinator.CheckRateLimitAsync(firstFunction, parameters)).ShouldBeFalse();
        (await coordinator.CheckRateLimitAsync(secondFunction, parameters)).ShouldBeTrue();
    }

    [Fact]
    public async Task RouteValues_Affect_Key()
    {
        using var scope = _scopeFactory.CreateScope();
        var coordinator = scope.ServiceProvider.GetRequiredService<IRateLimitCoordinator>();
        var ip = TestInitializer.GetRandomIpAddress();
        var parameters = new RateLimitEndPointParams { Limit = 1, PeriodInSec = 60, RouteParams = "id" };
        var firstRouteValue = CreateContext(ip, routeValues: new Dictionary<string, object> { { "id", "1" } });
        var secondRouteValue = CreateContext(ip, routeValues: new Dictionary<string, object> { { "id", "2" } });

        (await coordinator.CheckRateLimitAsync(firstRouteValue, parameters)).ShouldBeTrue();
        (await coordinator.CheckRateLimitAsync(firstRouteValue, parameters)).ShouldBeFalse();
        (await coordinator.CheckRateLimitAsync(secondRouteValue, parameters)).ShouldBeTrue();
    }

    [Fact]
    public async Task QueryValues_Affect_Key()
    {
        using var scope = _scopeFactory.CreateScope();
        var coordinator = scope.ServiceProvider.GetRequiredService<IRateLimitCoordinator>();
        var ip = TestInitializer.GetRandomIpAddress();
        var parameters = new RateLimitEndPointParams { Limit = 1, PeriodInSec = 60, QueryParams = "q" };
        var firstQueryValue = CreateContext(ip, queryValues: new Dictionary<string, IReadOnlyList<string>> { { "q", new[] { "a" } } });
        var secondQueryValue = CreateContext(ip, queryValues: new Dictionary<string, IReadOnlyList<string>> { { "q", new[] { "b" } } });

        (await coordinator.CheckRateLimitAsync(firstQueryValue, parameters)).ShouldBeTrue();
        (await coordinator.CheckRateLimitAsync(firstQueryValue, parameters)).ShouldBeFalse();
        (await coordinator.CheckRateLimitAsync(secondQueryValue, parameters)).ShouldBeTrue();
    }

    [Fact]
    public async Task WhiteListedIp_IsNotRateLimited()
    {
        using var scope = _scopeFactory.CreateScope();
        var coordinator = scope.ServiceProvider.GetRequiredService<IRateLimitCoordinator>();
        var context = CreateContext("8.8.8.8"); //same ip in white list ip in appsettings.json
        var parameters = new RateLimitEndPointParams { Limit = 1, PeriodInSec = 60 };

        (await coordinator.CheckRateLimitAsync(context, parameters)).ShouldBeTrue();
        (await coordinator.CheckRateLimitAsync(context, parameters)).ShouldBeTrue();
    }

    [Fact]
    public async Task WhiteListedClientId_IsNotRateLimited()
    {
        using var scope = _scopeFactory.CreateScope();
        var coordinator = scope.ServiceProvider.GetRequiredService<IRateLimitCoordinator>();
        var context = CreateContext(TestInitializer.GetRandomIpAddress(), clientId: "test-client"); //same client id in white list in appsettings.json
        var parameters = new RateLimitEndPointParams { Limit = 1, PeriodInSec = 60 };

        (await coordinator.CheckRateLimitAsync(context, parameters)).ShouldBeTrue();
        (await coordinator.CheckRateLimitAsync(context, parameters)).ShouldBeTrue();
    }

    [Fact]
    public async Task NoIpAndNoClientId_IsNotRateLimited()
    {
        using var scope = _scopeFactory.CreateScope();
        var coordinator = scope.ServiceProvider.GetRequiredService<IRateLimitCoordinator>();
        var context = CreateContext(userIp: null!);
        var parameters = new RateLimitEndPointParams { Limit = 1, PeriodInSec = 60 };

        (await coordinator.CheckRateLimitAsync(context, parameters)).ShouldBeTrue();
        (await coordinator.CheckRateLimitAsync(context, parameters)).ShouldBeTrue();
    }

    private static FunctionRateLimitContext CreateContext(string userIp,
        string functionName = "TestFunction",
        string? clientId = null,
        IDictionary<string, object>? routeValues = null,
        IDictionary<string, IReadOnlyList<string>>? queryValues = null)
    {
        return new FunctionRateLimitContext
        {
            Method = "GET",
            FunctionName = functionName,
            UserIp = userIp,
            ClientId = clientId,
            RouteValues = routeValues ?? new Dictionary<string, object>(),
            QueryValues = queryValues ?? new Dictionary<string, IReadOnlyList<string>>()
        };
    }
}
