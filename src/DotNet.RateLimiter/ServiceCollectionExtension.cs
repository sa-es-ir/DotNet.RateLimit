using AsyncKeyedLock;
using DotNet.RateLimiter.ActionFilters;
using DotNet.RateLimiter.Implementations;
using DotNet.RateLimiter.Interfaces;
using DotNet.RateLimiter.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System.Collections.Generic;

namespace DotNet.RateLimiter
{
    public static class ServiceCollectionExtension
    {
        public static void AddRateLimitService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RateLimitOptions>(configuration.GetSection("RateLimitOption"));
            services.AddScoped<RateLimitAttribute>();
            services.AddScoped<IRateLimitCoordinator, RateLimitCoordinator>();

            var options = new RateLimitOptions();
            configuration.GetSection("RateLimitOption").Bind(options);

            if (options.HasRedis)
            {
                services.AddScoped<IRateLimitService, RedisRateLimitService>();
                services.AddSingleton<IRateLimitBackgroundTaskQueue, RateLimitBackgroundTaskQueue>();
                services.AddHostedService<QueuedHostedService>();

                //configure redis 
                var redisConfig = ConfigurationOptions.Parse(options.RedisConnection);
                services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConfig));
                services.AddTransient<IDatabase>(provider => provider.GetRequiredService<IConnectionMultiplexer>().GetDatabase());

                //add red lock for distributed lock
                services.AddSingleton<IDistributedLockFactory>(provider =>
                {
                    var config = ConfigurationOptions.Parse(options.RedisConnection);

                    return RedLockFactory.Create(new List<RedLockMultiplexer>()
                    {
                        ConnectionMultiplexer.Connect(config)
                    });
                });
            }
            else
            {
                services.AddMemoryCache();
                services.AddSingleton(new AsyncKeyedLocker<string>(o =>
                {
                    o.PoolSize = 20;
                    o.PoolInitialFill = 1;
                }));
                services.AddScoped<IRateLimitService, InMemoryRateLimitService>();
            }
        }
    }
}
