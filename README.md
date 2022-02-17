# DotNet.RateLimiter

This is a RateLimit that works with ActionFilters! The approach designed to control requests rate for specific Action or Controller. The idea behind this solution is to solve the middleware problem because the [Middlewares](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-6.0) affects all requests, but with action filters you can limit some of the critical endpoints.
