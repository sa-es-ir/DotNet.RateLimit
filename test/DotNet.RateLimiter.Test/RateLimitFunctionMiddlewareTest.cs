using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using DotNet.RateLimiter.ActionFilters;
using DotNet.RateLimiter.Functions;
using DotNet.RateLimiter.Models;
using Microsoft.Azure.Functions.Worker;
using Shouldly;
using Xunit;

namespace DotNet.RateLimiter.Test;

public class RateLimitFunctionMiddlewareTest
{
    [Fact]
    public void ResolveRateLimitAttributes_ReadsMethodLevelAttributes()
    {
        var definition = new FakeFunctionDefinition
        {
            EntryPointValue = $"{typeof(DecoratedFunctionTarget).FullName}.{nameof(DecoratedFunctionTarget.DecoratedFunction)}",
            PathToAssemblyValue = typeof(DecoratedFunctionTarget).Assembly.Location
        };

        var attributes = RateLimitFunctionMiddleware.ResolveRateLimitAttributes(definition);

        attributes.Count.ShouldBe(1);
        attributes[0].Limit.ShouldBe(5);
        attributes[0].PeriodInSec.ShouldBe(60);
        attributes[0].RouteParams.ShouldBe("id");
        attributes[0].QueryParams.ShouldBe("q");
    }

    [Fact]
    public void ResolveRateLimitAttributes_ReturnsEmpty_WhenMethodHasNoAttributes()
    {
        var definition = new FakeFunctionDefinition
        {
            EntryPointValue = $"{typeof(DecoratedFunctionTarget).FullName}.{nameof(DecoratedFunctionTarget.PlainFunction)}",
            PathToAssemblyValue = typeof(DecoratedFunctionTarget).Assembly.Location
        };

        RateLimitFunctionMiddleware.ResolveRateLimitAttributes(definition).ShouldBeEmpty();
    }

    [Fact]
    public void BuildRateLimitContext_CopiesRequestAndBindingData()
    {
        var context = new FakeFunctionContext
        {
            BindingContextValue = new FakeBindingContext(new Dictionary<string, object?>
            {
                { "id", "42" }
            }),
            FunctionDefinitionValue = new FakeFunctionDefinition { NameValue = "MyFunction" }
        };
        var request = new FakeHttpRequestData(context, method: "POST");
        request.Headers.Add("X-Forwarded-For", "1.2.3.4, 5.6.7.8");
        request.Headers.Add("X-Client-Id", "client-1");
        request.QueryValue = new NameValueCollection
        {
            { "q", "a" },
            { "q", "b" },
            { "page", "2" }
        };

        var result = RateLimitFunctionMiddleware.BuildRateLimitContext(
            context,
            request,
            new RateLimitOptions { IpHeaderName = "X-Forwarded-For", ClientIdentifier = "X-Client-Id" });

        result.Method.ShouldBe("POST");
        result.FunctionName.ShouldBe("MyFunction");
        result.UserIp.ShouldBe("1.2.3.4");
        result.ClientId.ShouldBe("client-1");
        result.RouteValues["id"].ShouldBe("42");
        result.QueryValues["q"].ShouldBe(new[] { "a", "b" });
        result.QueryValues["page"].ShouldBe(new[] { "2" });
    }

    [Fact]
    public async Task WriteRateLimitResponse_WritesStatusCodeAndJsonBody()
    {
        var context = new FakeFunctionContext();
        var request = new FakeHttpRequestData(context);
        var options = new RateLimitOptions { HttpStatusCode = 429, ErrorMessage = "Rate limit Exceeded" };

        await RateLimitFunctionMiddleware.WriteRateLimitResponse(context, request, options);

        var response = context.GetInvocationResult().Value.ShouldBeOfType<FakeHttpResponseData>();
        response.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        response.Headers.GetValues("Content-Type").ShouldContain("application/json");

        response.Body.Position = 0;
        var body = await new StreamReader(response.Body).ReadToEndAsync();
        body.ShouldContain("\"Message\":\"Rate limit Exceeded\"");
        body.ShouldContain("\"Code\":429");
    }

    public class DecoratedFunctionTarget
    {
        [RateLimit(Limit = 5, PeriodInSec = 60, RouteParams = "id", QueryParams = "q")]
        public void DecoratedFunction()
        {
        }

        public void PlainFunction()
        {
        }
    }
}
