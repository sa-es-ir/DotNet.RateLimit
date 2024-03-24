using DotNet.RateLimiter.Interfaces;
using Microsoft.Extensions.DependencyInjection;
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

    [Fact]
    public void TestMethod()
    {
        using var scope = _scopeFactory.CreateScope();

        var rateLimitCoordinator = scope.ServiceProvider.GetRequiredService<IRateLimitCoordinator>();

    }
}
