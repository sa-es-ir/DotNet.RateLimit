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
        /// <summary>
        /// Adds rate limiting service using configuration. For Redis, it creates new connections.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration containing RateLimitOption section</param>
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

        /// <summary>
        /// Adds rate limiting service using configuration with existing Redis connection multiplexer.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration containing RateLimitOption section</param>
        /// <param name="connectionMultiplexer">Existing Redis connection multiplexer to use</param>
        public static void AddRateLimitService(this IServiceCollection services, IConfiguration configuration, IConnectionMultiplexer connectionMultiplexer)
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

                // Use the provided connection multiplexer
                services.AddSingleton(connectionMultiplexer);
                services.AddTransient<IDatabase>(provider => provider.GetRequiredService<IConnectionMultiplexer>().GetDatabase());

                // Create distributed lock factory with existing connection
                services.AddSingleton<IDistributedLockFactory>(provider =>
                {
                    return RedLockFactory.Create(new List<RedLockMultiplexer>()
                    {
                        (ConnectionMultiplexer)connectionMultiplexer
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

        /// <summary>
        /// Adds rate limiting service using configuration with existing Redis database.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration containing RateLimitOption section</param>
        /// <param name="database">Existing Redis database to use</param>
        public static void AddRateLimitService(this IServiceCollection services, IConfiguration configuration, IDatabase database)
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

                // Use the provided database and get its multiplexer for distributed lock
                services.AddSingleton(database);
                var multiplexer = database.Multiplexer;
                services.AddSingleton(multiplexer);

                // Create distributed lock factory with existing connection
                services.AddSingleton<IDistributedLockFactory>(provider =>
                {
                    return RedLockFactory.Create(new List<RedLockMultiplexer>()
                    {
                        (ConnectionMultiplexer)multiplexer
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
