using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using DotNet.RateLimiter.Models;
using DotNet.RateLimiter.Utilities;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace DotNet.RateLimiter.Test;

public class CustomResponseStructureTest
{
    private readonly IServiceScopeFactory _scopeFactory;

    public CustomResponseStructureTest(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    [Fact]
    public void ResponseBuilder_WithDefaultResponse_ReturnsDefaultStructure()
    {
        var options = new RateLimitOptions
        {
            HttpStatusCode = 429,
            ErrorMessage = "Rate limit Exceeded",
            ResponseStructure = null
        };

        var responseBody = RateLimitResponseBuilder.BuildResponse(options);

        // Verify default response structure
        var response = JsonSerializer.Deserialize<RateLimitResponse>(responseBody);
        response.ShouldNotBeNull();
        response.Code.ShouldBe(429);
        response.Message.ShouldBe("Rate limit Exceeded");
    }

    [Fact]
    public void ResponseBuilder_WithCustomResponseStructure_ReturnsCustomStructure()
    {
        var options = new RateLimitOptions
        {
            HttpStatusCode = 429,
            ErrorMessage = "Rate limit Exceeded",
            ResponseStructure = "{\"error\": {\"message\": \"$(ErrorMessage)\", \"code\": $(HttpStatusCode)}}"
        };

        var responseBody = RateLimitResponseBuilder.BuildResponse(options);

        // Verify custom response structure
        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;

        root.ValueKind.ShouldBe(JsonValueKind.Object);
        root.TryGetProperty("error", out var error).ShouldBeTrue();

        error.TryGetProperty("message", out var messageProp).ShouldBeTrue();
        messageProp.GetString().ShouldBe("Rate limit Exceeded");

        error.TryGetProperty("code", out var codeProp).ShouldBeTrue();
        codeProp.GetInt32().ShouldBe(429);
    }

    [Fact]
    public void ResponseBuilder_WithSimpleCustomStructure_ReturnsCorrectly()
    {
        var options = new RateLimitOptions
        {
            HttpStatusCode = 429,
            ErrorMessage = "Too many requests",
            ResponseStructure = "{\"data\": {\"message\": \"$(ErrorMessage)\", \"code\": $(HttpStatusCode)}}"
        };

        var responseBody = RateLimitResponseBuilder.BuildResponse(options);

        // Verify custom response structure
        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;

        root.ValueKind.ShouldBe(JsonValueKind.Object);
        root.TryGetProperty("data", out var data).ShouldBeTrue();

        data.TryGetProperty("message", out var messageProp).ShouldBeTrue();
        messageProp.GetString().ShouldBe("Too many requests");

        data.TryGetProperty("code", out var codeProp).ShouldBeTrue();
        codeProp.GetInt32().ShouldBe(429);
    }

    [Fact]
    public void ResponseBuilder_WithComplexCustomStructure_ReturnsCorrectly()
    {
        var options = new RateLimitOptions
        {
            HttpStatusCode = 429,
            ErrorMessage = "Request rate limit exceeded",
            ResponseStructure = "{\"success\": false, \"error\": {\"type\": \"RateLimitError\", \"message\": \"$(ErrorMessage)\", \"httpStatus\": $(HttpStatusCode)}}"
        };

        var responseBody = RateLimitResponseBuilder.BuildResponse(options);

        // Verify complex custom response structure
        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;

        root.ValueKind.ShouldBe(JsonValueKind.Object);

        root.TryGetProperty("success", out var successProp).ShouldBeTrue();
        successProp.GetBoolean().ShouldBeFalse();

        root.TryGetProperty("error", out var error).ShouldBeTrue();

        error.TryGetProperty("type", out var typeProp).ShouldBeTrue();
        typeProp.GetString().ShouldBe("RateLimitError");

        error.TryGetProperty("message", out var messageProp).ShouldBeTrue();
        messageProp.GetString().ShouldBe("Request rate limit exceeded");

        error.TryGetProperty("httpStatus", out var statusProp).ShouldBeTrue();
        statusProp.GetInt32().ShouldBe(429);
    }

    [Theory]
    [InlineData(429)]
    [InlineData(503)]
    [InlineData(500)]
    public void ResponseBuilder_WithCustomStatusCode_ReturnsCorrectStatus(int statusCode)
    {
        var options = new RateLimitOptions
        {
            HttpStatusCode = statusCode,
            ErrorMessage = "Rate limit exceeded",
            ResponseStructure = "{\"message\": \"$(ErrorMessage)\", \"code\": $(HttpStatusCode)}"
        };

        var responseBody = RateLimitResponseBuilder.BuildResponse(options);

        // Verify custom response structure with correct status
        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;

        root.ValueKind.ShouldBe(JsonValueKind.Object);
        root.TryGetProperty("code", out var codeProp).ShouldBeTrue();
        codeProp.GetInt32().ShouldBe(statusCode);
    }

    [Fact]
    public void ResponseBuilder_WithEmptyResponseStructure_ReturnsDefaultStructure()
    {
        var options = new RateLimitOptions
        {
            HttpStatusCode = 429,
            ErrorMessage = "Rate limit Exceeded",
            ResponseStructure = "" // Empty string
        };

        var responseBody = RateLimitResponseBuilder.BuildResponse(options);

        // Verify default response structure is used
        var response = JsonSerializer.Deserialize<RateLimitResponse>(responseBody);
        response.ShouldNotBeNull();
        response.Code.ShouldBe(429);
        response.Message.ShouldBe("Rate limit Exceeded");
    }

    [Fact]
    public void ResponseBuilder_WithWhitespaceResponseStructure_ReturnsDefaultStructure()
    {
        var options = new RateLimitOptions
        {
            HttpStatusCode = 429,
            ErrorMessage = "Rate limit Exceeded",
            ResponseStructure = "   " // Whitespace only
        };

        var responseBody = RateLimitResponseBuilder.BuildResponse(options);

        // Verify default response structure is used
        var response = JsonSerializer.Deserialize<RateLimitResponse>(responseBody);
        response.ShouldNotBeNull();
        response.Code.ShouldBe(429);
        response.Message.ShouldBe("Rate limit Exceeded");
    }

    [Fact]
    public async Task RateLimit_Integration_ReturnsCorrectStatusCode()
    {
        using var scope = _scopeFactory.CreateScope();
        var rateLimitAction = TestInitializer.CreateRateLimitFilter(scope, limit: 1, periodInSec: 60);

        var actionContext = TestInitializer.SetupActionContext(ip: TestInitializer.GetRandomIpAddress());
        var actionExecutingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            null!);

        // First request should pass
        await rateLimitAction.OnActionExecutionAsync(
            actionExecutingContext,
            () => TestInitializer.ActionExecutionDelegateNext(actionContext));
        actionExecutingContext.HttpContext.Response.StatusCode.ShouldBe((int)HttpStatusCode.OK);

        // Second request should be rate limited
        await rateLimitAction.OnActionExecutionAsync(
            actionExecutingContext,
            () => TestInitializer.ActionExecutionDelegateNext(actionContext));
        actionExecutingContext.HttpContext.Response.StatusCode.ShouldBe((int)HttpStatusCode.TooManyRequests);
        actionExecutingContext.HttpContext.Response.ContentType.ShouldBe("application/json");
    }
}