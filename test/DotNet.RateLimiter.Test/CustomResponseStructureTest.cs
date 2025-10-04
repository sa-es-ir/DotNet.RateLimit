using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using DotNet.RateLimiter.Models;
using DotNet.RateLimiter.Utilities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        var response = JsonConvert.DeserializeObject<RateLimitResponse>(responseBody);
        response.Should().NotBeNull();
        response!.Code.Should().Be(429);
        response.Message.Should().Be("Rate limit Exceeded");
        response.Status.Should().Be("TooManyRequests");
    }

    [Fact]
    public void ResponseBuilder_WithCustomResponseStructure_ReturnsCustomStructure()
    {
        var options = new RateLimitOptions
        {
            HttpStatusCode = 429,
            ErrorMessage = "Rate limit Exceeded",
            ResponseStructure = "{\"error\": {\"message\": \"$(ErrorMessage)\", \"code\": $(HttpStatusCode), \"status\": \"$(Status)\"}}"
        };

        var responseBody = RateLimitResponseBuilder.BuildResponse(options);

        // Verify custom response structure
        var response = JObject.Parse(responseBody);
        response.Should().NotBeNull();
        response["error"].Should().NotBeNull();
        response["error"]!["message"]!.Value<string>().Should().Be("Rate limit Exceeded");
        response["error"]!["code"]!.Value<int>().Should().Be(429);
        response["error"]!["status"]!.Value<string>().Should().Be("TooManyRequests");
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
        var response = JObject.Parse(responseBody);
        response.Should().NotBeNull();
        response["data"].Should().NotBeNull();
        response["data"]!["message"]!.Value<string>().Should().Be("Too many requests");
        response["data"]!["code"]!.Value<int>().Should().Be(429);
    }

    [Fact]
    public void ResponseBuilder_WithComplexCustomStructure_ReturnsCorrectly()
    {
        var options = new RateLimitOptions
        {
            HttpStatusCode = 429,
            ErrorMessage = "Request rate limit exceeded",
            ResponseStructure = "{\"success\": false, \"error\": {\"type\": \"RateLimitError\", \"message\": \"$(ErrorMessage)\", \"httpStatus\": $(HttpStatusCode), \"statusText\": \"$(Status)\"}}"
        };

        var responseBody = RateLimitResponseBuilder.BuildResponse(options);

        // Verify complex custom response structure
        var response = JObject.Parse(responseBody);
        response.Should().NotBeNull();
        response["success"]!.Value<bool>().Should().BeFalse();
        response["error"].Should().NotBeNull();
        response["error"]!["type"]!.Value<string>().Should().Be("RateLimitError");
        response["error"]!["message"]!.Value<string>().Should().Be("Request rate limit exceeded");
        response["error"]!["httpStatus"]!.Value<int>().Should().Be(429);
        response["error"]!["statusText"]!.Value<string>().Should().Be("TooManyRequests");
    }

    [Theory]
    [InlineData(429, "TooManyRequests")]
    [InlineData(503, "ServiceUnavailable")]
    [InlineData(500, "InternalServerError")]
    public void ResponseBuilder_WithCustomStatusCode_ReturnsCorrectStatus(int statusCode, string expectedStatus)
    {
        var options = new RateLimitOptions
        {
            HttpStatusCode = statusCode,
            ErrorMessage = "Rate limit exceeded",
            ResponseStructure = "{\"message\": \"$(ErrorMessage)\", \"code\": $(HttpStatusCode), \"status\": \"$(Status)\"}"
        };

        var responseBody = RateLimitResponseBuilder.BuildResponse(options);

        // Verify custom response structure with correct status
        var response = JObject.Parse(responseBody);
        response.Should().NotBeNull();
        response["code"]!.Value<int>().Should().Be(statusCode);
        response["status"]!.Value<string>().Should().Be(expectedStatus);
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
        var response = JsonConvert.DeserializeObject<RateLimitResponse>(responseBody);
        response.Should().NotBeNull();
        response!.Code.Should().Be(429);
        response.Message.Should().Be("Rate limit Exceeded");
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
        var response = JsonConvert.DeserializeObject<RateLimitResponse>(responseBody);
        response.Should().NotBeNull();
        response!.Code.Should().Be(429);
        response.Message.Should().Be("Rate limit Exceeded");
    }

    [Fact]
    public async Task RateLimit_Integration_ReturnsCorrectStatusCode()
    {
        using var scope = _scopeFactory.CreateScope();
        var rateLimitAction = TestInitializer.CreateRateLimitFilter(scope, limit: 1, periodInSec: 60);

        var actionContext = TestInitializer.SetupActionContext(ip: TestInitializer.GetRandomIpAddress());
        var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), null!);

        // First request should pass
        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, () => TestInitializer.ActionExecutionDelegateNext(actionContext));
        actionExecutingContext.HttpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.OK);

        // Second request should be rate limited
        await rateLimitAction.OnActionExecutionAsync(actionExecutingContext, () => TestInitializer.ActionExecutionDelegateNext(actionContext));
        actionExecutingContext.HttpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.TooManyRequests);
        actionExecutingContext.HttpContext.Response.ContentType.Should().Be("application/json");
    }
}
