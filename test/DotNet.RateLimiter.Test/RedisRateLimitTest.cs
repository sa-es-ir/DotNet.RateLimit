using Microsoft.Extensions.DependencyInjection;
using Xunit.DependencyInjection;

namespace DotNet.RateLimiter.Test;

[Startup(typeof(StartupRedis))]
public class RedisRateLimitTest : BaseRateLimitTest
{
    public RedisRateLimitTest(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
    }
}