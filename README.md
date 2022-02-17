# DotNet.RateLimiter

This is a RateLimit that works with ActionFilters! The approach designed to control requests rate for specific Action or Controller. The idea behind this solution is to solve the middleware problem because the [Middlewares](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-6.0) affects all requests, but with action filters you can limit some of the critical endpoints.

Rate Limit uses InMemory cache by default, but if you set up a Redis connection it will use Redis, it is recommended that use Redis to check the rate limit in distributed applications. By default it limits the IP address for control requests rate but you can set ClientId in request headers the header name is configurable.


|Platform|Version|
|---|---|
|TargetFramework| **net6.0**, **netstandard2.1** |

## How to add in DI
RateLimitOption in appsettings.json
```csharp
"RateLimitOption": {
    "EnableRateLimit": true, //Optional: if set false rate limit will disable, default is true
    "HttpStatusCode": 429, //Optional: default is 429
    "ErrorMessage": "Rate limit Exceeded", //Optional: default is Rate limit Exceeded
    "IpHeaderName": "X-Forwarded-For" //Optional: header name for get Ip address, default is X-Forwarded-For
    //"RedisConnection": "127.0.0.1:6379",
    //"IpWhiteList": ["::1"],
    //"ClientIdentifier": "X-Client-Id" // for get client id from request header if this present the rate limit will not use IP for limit requests
    //"ClientIdentifierWhiteList": ["test-client"]
  }
```
You can add RateLimit in Startup like this:
```csharp
using DotNet.RateLimiter;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRateLimitService(builder.Configuration);
```
## How to use
You can see the **Demo** project to know how to use in all scenarios.
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
### Use with Route parameters
```csharp
[HttpGet("by-route/{id}")]
[RateLimit(PeriodInSec = 60, Limit = 3, RouteParams = "id")]
public IEnumerable<WeatherForecast> Get(int id)
{
   ....
}
```
### Use with Query parameters
```csharp
[HttpGet("by-query/{id}")]
[RateLimit(PeriodInSec = 60, Limit = 3, RouteParams = "id", QueryParams = "name,family")]
public IEnumerable<WeatherForecast> Get(int id, string name, [FromQuery] List<string> family)
{
    ....
}
```
