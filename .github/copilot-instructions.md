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
- `dotnet restore` -- takes 20 seconds. NEVER CANCEL. Set timeout to 3+ minutes.
- `dotnet build` -- takes 10 seconds with 2 warnings (expected). NEVER CANCEL. Set timeout to 3+ minutes.
- `dotnet test` -- takes 15 seconds, runs 36 tests. NEVER CANCEL. Set timeout to 3+ minutes.
  - All tests pass successfully, validating rate limiting behavior works correctly
  - Tests verify rate limiting properly returns 429 status codes when limits are exceeded
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
appveyor.yml
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
Test summary: total: 36, failed: 0, succeeded: 36, skipped: 0
# All tests pass successfully - rate limiting behavior is validated correctly
```

### Troubleshooting
- If restore fails with "NETSDK1045": Install .NET 9.0 SDK using the commands above
- If tests fail with "Microsoft.AspNetCore.App not found": Install ASP.NET Core 8.0 runtime
- If demo app redirects to HTTPS: Use https://localhost:7196 endpoints instead of http://localhost:5196
- Always use `export PATH="$HOME/.dotnet:$PATH"` if .NET commands are not found
- Build warnings about nullable reference types are expected and do not affect functionality