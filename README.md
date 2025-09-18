<p align="center"><img src="rate-limit.webp" width="300"></p>

<div align="center">
	
[![Build Status](https://dev.azure.com/SaeedDevOps/Top_Engineering/_apis/build/status%2Fsa-es-ir.DotNet.RateLimit?branchName=refs%2Fpull%2F30%2Fmerge)](https://dev.azure.com/SaeedDevOps/Top_Engineering/_build/latest?definitionId=4&branchName=refs%2Fpull%2F30%2Fmerge)
[![NuGet](https://img.shields.io/nuget/v/DotNetRateLimiter.svg)](https://www.nuget.org/packages/DotNetRateLimiter/)
[![NuGet](https://img.shields.io/nuget/dt/DotNetRateLimiter.svg)](https://www.nuget.org/packages/DotNetRateLimiter/)
[![GitHub stars](https://img.shields.io/github/stars/SaeedEsmaeelinejad/DotNet.RateLimit.svg)](https://github.com/SaeedEsmaeelinejad/DotNet.RateLimit/stargazers)
</div>

# DotNetRateLimiter

This is a RateLimit that works with ActionFilters and EndPointFilters! An approach designed to control the request rate for a specific Action, Controller, or minimal endpoint. The idea behind this solution is to solve the middleware problem because the [Middlewares](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-6.0) affects all requests, but with filters, you can limit some of the critical endpoints.

Rate Limit uses **In-Memory** cache by default, but if you set up a **Redis** connection it will use Redis, it is recommended to use Redis for checking the rate limit in the distributed applications. By default, it limits the IP address but you can set ClientId in request headers and the header name is configurable.

## Thank you for giving a ⭐ to this repo!

## How to add in DI
You can add RateLimit in Startup like this:
```csharp
using DotNet.RateLimiter;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRateLimitService(builder.Configuration);
```

### Using Existing Redis Connections

If your application already has Redis infrastructure, you can reuse existing connections instead of creating new ones:

```csharp
using DotNet.RateLimiter;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Setup your existing Redis connection
var multiplexer = ConnectionMultiplexer.Connect("your-connection-string");
builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);

// Use existing connection with rate limiting
builder.Services.AddRateLimitService(builder.Configuration, multiplexer);

// Or use existing database
var database = multiplexer.GetDatabase();
builder.Services.AddRateLimitService(builder.Configuration, database);
```

📖 **[Read more about using existing Redis connections](USAGE_EXISTING_REDIS.md)**
## How to use
You can see the **Demo** project to know how to use it in all scenarios and also you can follow this article in [**Medium**](https://medium.com/@s.esmaeelinejad/net-6-ratelimit-with-actionfilters-918a1aacb5fa).
### Simple usage
Using RateLimit without any parameters
```csharp
[HttpGet("")]
[RateLimit(PeriodInSec = 60, Limit = 3)]
public IEnumerable<WeatherForecast> Get()
{
    ....
}
```
For MinimalAPI **.NET 7+**
```csharp
app.MapGet("/weatherforecast", () =>
{
    return Results.Ok();
})
.WithRateLimiter(options =>
{
    options.PeriodInSec = 60;
    options.Limit = 3;
});
```
### Using Route parameters
```csharp
[HttpGet("by-route/{id}")]
[RateLimit(PeriodInSec = 60, Limit = 3, RouteParams = "id")]
public IEnumerable<WeatherForecast> Get(int id)
{
   ....
}
```
For MinimalAPI **.NET 7+**
```csharp
app.MapGet("/weatherforecast/{id}", (int id) =>
{
    return Results.Ok();
})
.WithRateLimiter(options =>
{
    options.PeriodInSec = 60;
    options.Limit = 3;
    options.RouteParams = "id";
});
```
### Using Query parameters
```csharp
[HttpGet("by-query/{id}")]
[RateLimit(PeriodInSec = 60, Limit = 3, RouteParams = "id", QueryParams = "name,family")]
public IEnumerable<WeatherForecast> Get(int id, string name, [FromQuery] List<string> family)
{
    ....
}
```
For MinimalAPI **.NET 7+**
```csharp
app.MapGet("/weatherforecast/{id}", (int id, string name, string family) =>
{
    return Results.Ok();
})
.WithRateLimiter(options =>
{
    options.PeriodInSec = 60;
    options.Limit = 2;
    options.QueryParams = "name,family";
    options.RouteParams = "id";
});
```
### Using with Body parameters (Only for ActionFilters)
```csharp
// body parameter only works on root parameters and does not work on nested parameters.
[HttpPut]
[RateLimit(PeriodInSec = 60, Limit = 3, BodyParams = "temperatureC" )]
public IActionResult Update([FromBody] WeatherForecast weatherForecast)
{
	....
}

....
  public class WeatherForecast
    {
        public int TemperatureC { get; set; }
    }
```
### Using on Controller
```csharp
//if Scope set to Controller then the rate limit works on all actions and no matter which actions call
//the default value is Action which means the rate limit checks each action separately
[RateLimit(Limit = 3, PeriodInSec = 60, Scope = RateLimitScope.Controller)]
public class RateLimitOnAllController : ControllerBase
{ .... }
```
### Ignoring rate limit in case of use on Controller
```csharp
[RateLimit(Limit = 3, PeriodInSec = 60, Scope = RateLimitScope.Controller)]
public class RateLimitOnAllController : ControllerBase
{
    [HttpGet("")]
    [IgnoreRateLimit]//ignore rate limit for this action
    public IEnumerable<WeatherForecast> Get()
    {
      ....
    }
}
```
### Custom configuration
RateLimitOption in appsettings.json
```csharp
"RateLimitOption": {
    "EnableRateLimit": true, //Optional: if set false rate limit will be disabled, default is true
    "HttpStatusCode": 429, //Optional: default is 429
    "ErrorMessage": "Rate limit Exceeded", //Optional: default is Rate limit Exceeded
    "IpHeaderName": "X-Forwarded-For" //Optional: header name for getting the IP address, default is X-Forwarded-For
    "RedisConnection": "127.0.0.1:6379", //Optional
    "IpWhiteList": ["::1"], //Optional
    "ClientIdentifier": "X-Client-Id" //Optional: for getting client id from the request header if this is present the rate limit won't use IP but ClientId
    "ClientIdentifierWhiteList": ["test-client"] //Optional
  }
```
