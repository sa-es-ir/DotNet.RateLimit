using AsyncKeyedLock;
using DotNet.RateLimiter.ActionFilters;
using DotNet.RateLimiter.Implementations;
using DotNet.RateLimiter.Interfaces;
using DotNet.RateLimiter.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

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
                //configure redis 
                var redisConfig = ConfigurationOptions.Parse(options.RedisConnection);
                services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConfig));
                services.AddTransient<IDatabase>(provider =>
                    provider.GetRequiredService<IConnectionMultiplexer>().GetDatabase());
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
        public static void AddRateLimitService(this IServiceCollection services, IConfiguration configuration,
            IConnectionMultiplexer connectionMultiplexer)
        {
            services.Configure<RateLimitOptions>(configuration.GetSection("RateLimitOption"));
            services.AddScoped<RateLimitAttribute>();
            services.AddScoped<IRateLimitCoordinator, RateLimitCoordinator>();

            services.AddScoped<IRateLimitService, RedisRateLimitService>();

            // Use the provided connection multiplexer - only add if not already registered
            services.TryAddSingleton(connectionMultiplexer);
            services.TryAddTransient<IDatabase>(provider =>
                provider.GetRequiredService<IConnectionMultiplexer>().GetDatabase());
        }

        /// <summary>
        /// Adds rate limiting service using configuration with existing Redis database.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration containing RateLimitOption section</param>
        /// <param name="database">Existing Redis database to use</param>
        public static void AddRateLimitService(this IServiceCollection services, IConfiguration configuration,
            IDatabase database)
        {
            services.Configure<RateLimitOptions>(configuration.GetSection("RateLimitOption"));
            services.AddScoped<RateLimitAttribute>();
            services.AddScoped<IRateLimitCoordinator, RateLimitCoordinator>();

            services.AddScoped<IRateLimitService, RedisRateLimitService>();

            // Use the provided database and get its multiplexer for distributed lock - only add if not already registered
            services.TryAddSingleton(database);
            var multiplexer = database.Multiplexer;
            services.TryAddSingleton(multiplexer);
        }
    }
}