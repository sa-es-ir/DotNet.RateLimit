# DotNet.RateLimiter

This is a RateLimit that works with ActionFilters! The approach designed to control requests rate for specific Action or Controller. The idea behind this solution is to solve the middleware problem because the [Middlewares](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-6.0) affects all requests, but with action filters you can limit some of the critical endpoints.

Rate Limit uses InMemory cache by default, but if you set up a Redis connection it will use Redis, it is recommended that use Redis to check the rate limit in distributed applications.


|Platform|Version|
|---|---|
|TargetFramework| **net6.0**, **netstandard2.1** |

## How to add in DI
You can add RateLimit in Startup like this:
```csharp
using DotNet.RateLimiter;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRateLimitService(builder.Configuration);
```
## How to use
You can see the **Demo** project and see how to use in code.
### Simple use
Use RateLimit with any parameters
```csharp
[HttpGet("")]
[RateLimit(PeriodInSec = 60, Limit = 3)]
public IEnumerable<WeatherForecast> Get()
{
    ....
}
```
