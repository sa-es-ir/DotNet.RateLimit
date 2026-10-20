#if NET8_0_OR_GREATER
using DotNet.RateLimiter.ActionFilters;
using DotNet.RateLimiter.Extensions;
using DotNet.RateLimiter.Interfaces;
using DotNet.RateLimiter.Models;
using DotNet.RateLimiter.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace DotNet.RateLimiter.Functions;

/// <summary>
/// Azure Functions isolated worker middleware that enforces the <see cref="RateLimit"/>
/// attribute placed on HttpTrigger functions.
/// </summary>
public class RateLimitFunctionMiddleware : IFunctionsWorkerMiddleware
{
    private readonly IOptions<RateLimitOptions> _options;
    private readonly IRateLimitCoordinator _rateLimitCoordinator;

    public RateLimitFunctionMiddleware(IOptions<RateLimitOptions> options, IRateLimitCoordinator rateLimitCoordinator)
    {
        _options = options;
        _rateLimitCoordinator = rateLimitCoordinator;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var handled = await TryHandleRateLimitAsync(context);

        if (!handled)
            await next(context);
    }

    private async Task<bool> TryHandleRateLimitAsync(FunctionContext context)
    {
        try
        {
            var request = await context.GetHttpRequestDataAsync();
            if (request is null)
                return false;

            var attributes = ResolveRateLimitAttributes(context.FunctionDefinition);
            if (attributes.Count == 0)
                return false;

            foreach (var attribute in attributes)
            {
                var rateLimitContext = BuildRateLimitContext(context, request, _options.Value);
                var rateLimitParams = new RateLimitEndPointParams
                {
                    PeriodInSec = attribute.PeriodInSec,
                    Limit = attribute.Limit,
                    RouteParams = attribute.RouteParams,
                    QueryParams = attribute.QueryParams
                };

                if (!await _rateLimitCoordinator.CheckRateLimitAsync(rateLimitContext, rateLimitParams))
                {
                    await WriteRateLimitResponse(context, request, _options.Value);
                    return true;
                }
            }

            return false;
        }
        catch
        {
            //rate limiting must never take a function down
            return false;
        }
    }

    internal static IReadOnlyList<RateLimit> ResolveRateLimitAttributes(FunctionDefinition functionDefinition)
    {
        try
        {
            if (functionDefinition is null ||
                string.IsNullOrWhiteSpace(functionDefinition.EntryPoint) ||
                string.IsNullOrWhiteSpace(functionDefinition.PathToAssembly))
            {
                return Array.Empty<RateLimit>();
            }

            var entryPoint = functionDefinition.EntryPoint;
            var separatorIndex = entryPoint.LastIndexOf('.');
            if (separatorIndex <= 0)
                return Array.Empty<RateLimit>();

            var typeName = entryPoint.Substring(0, separatorIndex);
            var methodName = entryPoint.Substring(separatorIndex + 1);

            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => string.Equals(a.Location, functionDefinition.PathToAssembly, StringComparison.OrdinalIgnoreCase))
                ?? Assembly.LoadFrom(functionDefinition.PathToAssembly);

            var type = assembly.GetType(typeName);
            var method = type?.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == methodName);

            if (method is null)
                return Array.Empty<RateLimit>();

            return method.GetCustomAttributes(typeof(RateLimit), inherit: true)
                .OfType<RateLimit>()
                .ToArray();
        }
        catch
        {
            return Array.Empty<RateLimit>();
        }
    }

    internal static FunctionRateLimitContext BuildRateLimitContext(
        FunctionContext context,
        HttpRequestData request,
        RateLimitOptions options)
    {
        var routeValues = new Dictionary<string, object>();
        if (context.BindingContext?.BindingData is not null)
        {
            foreach (var item in context.BindingContext.BindingData)
                routeValues[item.Key] = item.Value;
        }

        var queryValues = new Dictionary<string, IReadOnlyList<string>>();
        if (request.Query is not null)
        {
            foreach (var key in request.Query.AllKeys)
            {
                if (key is null)
                    continue;

                queryValues[key] = request.Query.GetValues(key) ?? Array.Empty<string>();
            }
        }

        return new FunctionRateLimitContext
        {
            Method = request.Method,
            FunctionName = context.FunctionDefinition?.Name,
            UserIp = GetUserIp(request, options?.IpHeaderName),
            ClientId = GetHeaderValue(request, options?.ClientIdentifier),
            RouteValues = routeValues,
            QueryValues = queryValues
        };
    }

    internal static async Task WriteRateLimitResponse(
        FunctionContext context,
        HttpRequestData request,
        RateLimitOptions options)
    {
        var response = request.CreateResponse();
        response.StatusCode = (HttpStatusCode)options.HttpStatusCode;
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync(RateLimitResponseBuilder.BuildResponse(options));

        context.GetInvocationResult().Value = response;
    }

    private static string GetUserIp(HttpRequestData request, string headerName)
    {
        var headerValue = GetHeaderValue(request, headerName);
        return headerValue?.GetIpAddresses()?.FirstOrDefault()?.ToString();
    }

    private static string GetHeaderValue(HttpRequestData request, string headerName)
    {
        if (string.IsNullOrWhiteSpace(headerName) || request.Headers is null)
            return null;

        return request.Headers.TryGetValues(headerName, out var values)
            ? string.Join(",", values)
            : null;
    }
}
#endif
