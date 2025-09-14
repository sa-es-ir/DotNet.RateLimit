using DotNet.RateLimiter.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System.Threading.Tasks;
using Testcontainers.Redis;
using Xunit;
using Xunit.DependencyInjection;

namespace DotNet.RateLimiter.Test;

[Startup(typeof(StartupWithExistingRedis))]
public class ExistingRedisConnectionTest : BaseRateLimitTest
{
    public ExistingRedisConnectionTest(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
    }
}

public class StartupWithExistingRedis
{
    private static IConnectionMultiplexer? _sharedMultiplexer;
    private static RedisTestContainer? _sharedRedisContainer;

    public void ConfigureServices(IServiceCollection services, HostBuilderContext context)
    {
        // Set up Redis container first
        if (_sharedRedisContainer == null)
        {
            _sharedRedisContainer = new RedisTestContainer();
            _sharedRedisContainer.InitializeAsync().Wait();
        }

        // Create existing Redis connection
        if (_sharedMultiplexer == null)
        {
            _sharedMultiplexer = ConnectionMultiplexer.Connect(_sharedRedisContainer.ConnectionString);
        }

        // Update configuration to enable Redis
        context.Configuration["RateLimitOption:RedisConnection"] = _sharedRedisContainer.ConnectionString;

        // Add rate limiting with existing connection
        services.AddRateLimitService(context.Configuration, _sharedMultiplexer);
    }

    public void ConfigureHost(IHostBuilder hostBuilder) =>
        hostBuilder.ConfigureAppConfiguration((_, builder) =>
        {
            builder.AddJsonFile("appsettings_redis.json");
        });
}