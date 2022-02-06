using DotNet.RateLimit.ActionFilters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotNet.RateLimit
{
    public static class ServiceCollectionExtension
    {
        public static void AddRateLimitService(this IServiceCollection services, IConfigurationSection configuration)
        {
            services.Configure<RateLimitOptions>(configuration);
            services.AddScoped<IRateLimitService, RateLimitService>();
            services.AddSingleton<IRateLimitBackgroundTaskQueue, RateLimitBackgroundTaskQueue>();
            services.AddHostedService<QueuedHostedService>();
            services.AddScoped<RateLimitAttribute>();

        }
    }
}