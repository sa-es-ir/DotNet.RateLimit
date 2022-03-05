using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

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
        services.AddRateLimitService(context.Configuration);
    }

    public void ConfigureHost(IHostBuilder hostBuilder) =>
        hostBuilder.ConfigureAppConfiguration((_, builder) =>
        {
            builder.AddJsonFile("appsettings_redis.json");
        });

}