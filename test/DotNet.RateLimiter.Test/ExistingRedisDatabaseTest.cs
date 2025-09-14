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

[Startup(typeof(StartupWithExistingDatabase))]
public class ExistingRedisDatabaseTest : BaseRateLimitTest
{
    public ExistingRedisDatabaseTest(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
    }
}

public class StartupWithExistingDatabase
{
    private static IDatabase? _sharedDatabase;
    private static RedisTestContainer? _sharedRedisContainer;

    public void ConfigureServices(IServiceCollection services, HostBuilderContext context)
    {
        // Set up Redis container first
        if (_sharedRedisContainer == null)
        {
            _sharedRedisContainer = new RedisTestContainer();
            _sharedRedisContainer.InitializeAsync().Wait();
        }

        // Create existing Redis database
        if (_sharedDatabase == null)
        {
            var multiplexer = ConnectionMultiplexer.Connect(_sharedRedisContainer.ConnectionString);
            _sharedDatabase = multiplexer.GetDatabase();
        }

        // Update configuration to enable Redis
        context.Configuration["RateLimitOption:RedisConnection"] = _sharedRedisContainer.ConnectionString;

        // Add rate limiting with existing database
        services.AddRateLimitService(context.Configuration, _sharedDatabase);
    }

    public void ConfigureHost(IHostBuilder hostBuilder) =>
        hostBuilder.ConfigureAppConfiguration((_, builder) =>
        {
            builder.AddJsonFile("appsettings_redis.json");
        });
}