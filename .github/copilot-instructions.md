# DotNet.RateLimit
DotNet.RateLimit is a .NET library that provides rate limiting functionality for ASP.NET Core applications using ActionFilters and EndPointFilters. It supports both in-memory caching and Redis for distributed scenarios, and works with both traditional controllers and minimal APIs.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Install Required .NET Versions
Always install both .NET 9.0 SDK and .NET 8.0 runtime components first:
- `curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0`
- `curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0 --runtime dotnet`
- `curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0 --runtime aspnetcore`
- `export PATH="$HOME/.dotnet:$PATH"` (required for each terminal session)

### Bootstrap, Build and Test the Repository
- `dotnet restore` -- takes 20-25 seconds. NEVER CANCEL. Set timeout to 3+ minutes.
- `dotnet build` -- takes 10 seconds with 2 warnings (expected). NEVER CANCEL. Set timeout to 3+ minutes.
- `dotnet test` -- takes 10-15 seconds, runs 66 tests. NEVER CANCEL. Set timeout to 3+ minutes.
  - All tests pass successfully, validating rate limiting behavior works correctly
  - Tests verify rate limiting properly returns 429 status codes when limits are exceeded
  - Tests use Docker containers (via Testcontainers) for Redis integration tests
- `dotnet pack src/DotNet.RateLimiter/DotNet.RateLimiter.csproj -c Release` -- takes 4 seconds for packaging

### Run the Demo Application
- ALWAYS run the bootstrapping steps first (install .NET, restore packages)
- `cd demo/DotNet.RateLimiter.Demo && dotnet run`
- Application runs on:
  - HTTP: http://localhost:5196
  - HTTPS: https://localhost:7196 (with untrusted dev certificate)
- Swagger UI available at: https://localhost:7196/swagger/index.html

## Validation
- ALWAYS manually validate rate limiting functionality after making changes
- Test rate limiting endpoints:
  - `curl -k https://localhost:7196/rate-limit-on-actions` (first 3 requests should return 200)
  - `curl -k https://localhost:7196/rate-limit-on-actions` (4th+ requests should return 429)
- ALWAYS test with the demo application to ensure end-to-end functionality works
- ALWAYS run all tests with `dotnet test` and verify all tests pass successfully

## Common Tasks

### Project Structure
The repository contains:
- `src/DotNet.RateLimiter/` - Main library project (multi-targets: netstandard2.0, netstandard2.1, net8.0, net9.0)
- `test/DotNet.RateLimiter.Test/` - Unit tests (targets net8.0)
- `demo/DotNet.RateLimiter.Demo/` - Demo web application (targets net9.0)
- `DotNet.RateLimit.sln` - Solution file with all projects

### Key Configuration Files
- `src/DotNet.RateLimiter/DotNet.RateLimiter.csproj` - Main library project file with multi-targeting
- `demo/DotNet.RateLimiter.Demo/appsettings.json` - Demo app configuration with RateLimitOption settings
- `.github/workflows/package.yml` - CI/CD pipeline for packaging and releases
- `azure-pipelines.yml` - Azure DevOps CI/CD pipeline configuration

### Configuration Options (appsettings.json)
```json
"RateLimitOption": {
    "EnableRateLimit": true,           // Optional: default is true
    "HttpStatusCode": 429,              // Optional: default is 429
    "ErrorMessage": "Rate limit Exceeded", // Optional
    "IpHeaderName": "X-Forwarded-For",  // Optional: default is X-Forwarded-For
    "RedisConnection": "127.0.0.1:6379", // Optional: for distributed scenarios
    "IpWhiteList": ["::1"],             // Optional: IPs to exclude from rate limiting
    "ClientIdentifier": "X-Client-Id",  // Optional: use Client ID instead of IP
    "ClientIdentifierWhiteList": ["test-client"], // Optional
    "ResponseStructure": "{\"error\": {\"message\": \"$(ErrorMessage)\", \"code\": $(HttpStatusCode)}}" // Optional: custom response format
}
```

### Build Requirements
- .NET 9.0 SDK required for building all targets
- .NET 8.0 runtime required for running tests (ASP.NET Core 8.0)
- Multi-target build supports: netstandard2.0, netstandard2.1, net8.0, net9.0

### Rate Limiting Features Tested
- IP-based rate limiting (default)
- Route parameter-based rate limiting (`RouteParams`)
- Query parameter-based rate limiting (`QueryParams`)  
- Body parameter-based rate limiting (`BodyParams`)
- Controller-level rate limiting (`RateLimitScope.Controller`)
- White-listing (IP and Client ID)
- Redis distributed caching support
- In-memory caching (default)
- Custom response structures with placeholders (`$(ErrorMessage)`, `$(HttpStatusCode)`)

### Key Features
- **ActionFilters and EndpointFilters**: Works with both traditional controllers and minimal APIs (.NET 7+)
- **Redis Support**: Use existing Redis connections or create new ones via configuration
- **Flexible Rate Limiting**: Based on IP, Client ID, route params, query params, or body params
- **Custom Response Structures**: Customize JSON error responses when rate limits are exceeded
- **White-listing**: Exclude specific IPs or Client IDs from rate limiting
- **Distributed Scenarios**: Redis recommended for multi-instance deployments

### Key Endpoints in Demo App
- `GET /rate-limit-on-actions` - Basic rate limiting (3 requests per 60 seconds)
- `GET /rate-limit-on-actions/by-route/{id}` - Route parameter-based limiting
- `GET /rate-limit-on-actions/by-query/{id}?name=X&family=Y` - Query parameter-based limiting
- `PUT /rate-limit-on-actions` - Body parameter-based limiting

### Repository Commands Output Reference
```bash
# Repository root structure
ls -la
.git
.github
.gitignore
DotNet.RateLimit.sln
LICENSE
README.md
USAGE_EXISTING_REDIS.md
azure-pipelines.yml
demo/
rate-limit.webp
src/
test/

# Solution projects
dotnet sln list
src/DotNet.RateLimiter/DotNet.RateLimiter.csproj
test/DotNet.RateLimiter.Test/DotNet.RateLimiter.Test.csproj
demo/DotNet.RateLimiter.Demo/DotNet.RateLimiter.Demo.csproj

# Test summary (expected results)
dotnet test
Test summary: total: 66, failed: 0, succeeded: 66, skipped: 0
# All tests pass successfully - rate limiting behavior is validated correctly
# Tests include both in-memory and Redis-based rate limiting scenarios
```

### Troubleshooting
- If restore fails with "NETSDK1045": Install .NET 9.0 SDK using the commands above
- If tests fail with "Microsoft.AspNetCore.App not found": Install ASP.NET Core 8.0 runtime
- If demo app redirects to HTTPS: Use https://localhost:7196 endpoints instead of http://localhost:5196
- Always use `export PATH="$HOME/.dotnet:$PATH"` if .NET commands are not found
- Build warnings about nullable reference types are expected and do not affect functionality
- Tests require Docker for Redis integration tests (Testcontainers)

## Code Patterns

### Adding Rate Limiting to DI Container
```csharp
// Basic usage - creates new Redis connections if configured
builder.Services.AddRateLimitService(builder.Configuration);

// Using existing Redis connection (recommended for shared infrastructure)
var multiplexer = ConnectionMultiplexer.Connect("connection-string");
builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);
builder.Services.AddRateLimitService(builder.Configuration, multiplexer);

// Or use existing database
var database = multiplexer.GetDatabase();
builder.Services.AddRateLimitService(builder.Configuration, database);
```

### Usage Examples

**Controller Actions:**
```csharp
[HttpGet("")]
[RateLimit(PeriodInSec = 60, Limit = 3)]
public IActionResult Get() { ... }

[HttpGet("by-route/{id}")]
[RateLimit(PeriodInSec = 60, Limit = 3, RouteParams = "id")]
public IActionResult Get(int id) { ... }

[HttpGet("by-query/{id}")]
[RateLimit(PeriodInSec = 60, Limit = 3, RouteParams = "id", QueryParams = "name,family")]
public IActionResult Get(int id, string name, [FromQuery] List<string> family) { ... }

[HttpPut]
[RateLimit(PeriodInSec = 60, Limit = 3, BodyParams = "temperature")]
public IActionResult Update([FromBody] WeatherForecast forecast) { ... }
```

**Minimal APIs (.NET 7+):**
```csharp
app.MapGet("/weatherforecast", () => Results.Ok())
   .WithRateLimiter(options => {
       options.PeriodInSec = 60;
       options.Limit = 3;
   });

app.MapGet("/weatherforecast/{id}", (int id) => Results.Ok())
   .WithRateLimiter(options => {
       options.PeriodInSec = 60;
       options.Limit = 2;
       options.RouteParams = "id";
   });
```

**Controller-Level Rate Limiting:**
```csharp
[ApiController]
[RateLimit(PeriodInSec = 60, Limit = 10, Scope = RateLimitScope.Controller)]
public class MyController : ControllerBase { ... }
```