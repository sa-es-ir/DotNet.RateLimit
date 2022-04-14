<p align="center"><img src="rate-limit.webp" width="300"></p>


# DotNetRateLimiter

This is a RateLimit that works with ActionFilters! An approach designed to control the request rate for a particular Action or Controller. The idea behind this solution is to solve the middleware problem because the [Middlewares](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-6.0) affects all requests, but with action filters you can limit some of the critical endpoints.

Rate Limit uses InMemory cache by default, but if you set up a Redis connection it will use Redis, it is recommended that use Redis to check the rate limit in distributed applications. By default it limits the IP address for control requests rate but you can set ClientId in request headers the header name is configurable.

[![Build status](https://ci.appveyor.com/api/projects/status/9i2u298skqni6s3g?svg=true)](https://ci.appveyor.com/project/SaeedEsmaeelinejad/dotnet-ratelimit)
[![NuGet](https://img.shields.io/nuget/v/DotNetRateLimiter.svg)](https://www.nuget.org/packages/DotNetRateLimiter/)
[![GitHub stars](https://img.shields.io/github/stars/SaeedEsmaeelinejad/DotNet.RateLimit.svg)](https://github.com/SaeedEsmaeelinejad/DotNet.RateLimit/stargazers)
[![GitHub forks](https://img.shields.io/github/forks/SaeedEsmaeelinejad/DotNet.RateLimit.svg)](https://github.com/SaeedEsmaeelinejad/DotNet.RateLimit/network)

|TargetFramework|Support|
|---|---|
|**net6.0**|:white_check_mark:|
|**net5.0**|:white_check_mark:|
|**netcoreapp3.1**|:white_check_mark:|
|**netstandard2.1**|:white_check_mark:|
|**netstandard2.0**|:white_check_mark:|

## How to add in DI
RateLimitOption in appsettings.json
```csharp
"RateLimitOption": {
    "EnableRateLimit": true, //Optional: if set false rate limit will be disabled, default is true
    "HttpStatusCode": 429, //Optional: default is 429
    "ErrorMessage": "Rate limit Exceeded", //Optional: default is Rate limit Exceeded
    "IpHeaderName": "X-Forwarded-For" //Optional: header name for get Ip address, default is X-Forwarded-For
    "RedisConnection": "127.0.0.1:6379", //Optional
    "IpWhiteList": ["::1"], //Optional
    "ClientIdentifier": "X-Client-Id" ////Optional: for getting client id from request header if this present the rate limit will not use IP for limit requests
    "ClientIdentifierWhiteList": ["test-client"] ////Optional
  }
```
You can add RateLimit in Startup like this:
```csharp
using DotNet.RateLimiter;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRateLimitService(builder.Configuration);
```
## How to use
You can see the **Demo** project to know how to use in all scenarios and also you can follow this article in Medium.
### Simple use
Use RateLimit without any parameters
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
### Use on Controller
```csharp
//if Scope set to Controller to rate limit work on all actions no matter which actions call
//the default value is Action means this rate limit check for each action separately
[RateLimit(Limit = 3, PeriodInSec = 60, Scope = RateLimitScope.Controller)]
public class RateLimitOnAllController : ControllerBase
{ .... }
```
### Ignore rate limit in case of use on Controller
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
