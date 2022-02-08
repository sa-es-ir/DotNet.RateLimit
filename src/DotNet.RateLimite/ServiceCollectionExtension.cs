using DotNet.RateLimit.ActionFilters;
using DotNet.RateLimit.Implementations;
using DotNet.RateLimit.Interfaces;
using DotNet.RateLimit.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace DotNet.RateLimit
{
    public static class ServiceCollectionExtension
    {
        public static void AddRateLimitService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RateLimitOptions>(configuration.GetSection("RateLimitOption"));
            services.AddScoped<RateLimitAttribute>();

            var options = new RateLimitOptions();
            configuration.GetSection("RateLimitOptions").Bind(options);

            if (options.HasRedis)
            {
                services.AddScoped<IRateLimitService, RedisRateLimitService>();
                services.AddSingleton<IRateLimitBackgroundTaskQueue, RateLimitBackgroundTaskQueue>();
                services.AddHostedService<QueuedHostedService>();

                //configure redis 
                var redisConfig = ConfigurationOptions.Parse(configuration["RateLimitOption:RedisConnection"]);
                services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConfig));
                services.AddTransient<IDatabase>(provider => provider.GetRequiredService<IConnectionMultiplexer>().GetDatabase());
            }
            else
            {
                services.AddMemoryCache();
                services.AddScoped<IRateLimitService, InMemoryRateLimitService>();
            }
        }
    }
}