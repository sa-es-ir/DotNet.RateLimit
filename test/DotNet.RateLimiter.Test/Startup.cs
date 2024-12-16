using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using Testcontainers.Redis;

namespace DotNet.RateLimiter.Test;

public class Startup
{
    public void ConfigureServices(IServiceCollection services, HostBuilderContext context)
    {
        services.AddRateLimitService(context.Configuration);
    }

    public void ConfigureHost(IHostBuilder hostBuilder) =>
        hostBuilder.ConfigureAppConfiguration((_, builder) =>
        {
            builder.AddJsonFile("appsettings.json");
        });

}

public class StartupRedis
{
    public void ConfigureServices(IServiceCollection services, HostBuilderContext context)
    {
        var redis = new RedisTestContainer();
        redis.InitializeAsync().Wait();// runs once and ok to wait

        context.Configuration["RateLimitOption:RedisConnection"] = redis.ConnectionString;

        services.AddRateLimitService(context.Configuration);
    }

    public void ConfigureHost(IHostBuilder hostBuilder) =>
        hostBuilder.ConfigureAppConfiguration((_, builder) =>
        {
            builder.AddJsonFile("appsettings_redis.json");
        });

}

public class RedisTestContainer : IAsyncDisposable
{
    private readonly RedisContainer _redisContainer;

    public string ConnectionString => _redisContainer.GetConnectionString();

    public RedisTestContainer()
    {
        _redisContainer = new RedisBuilder()
            .WithImage("redis:7.4")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _redisContainer.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _redisContainer.StopAsync();
    }
}