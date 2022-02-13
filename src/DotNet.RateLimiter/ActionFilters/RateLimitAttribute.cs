﻿using System;
using System.Threading.Tasks;
using DotNet.RateLimiter.Extensions;
using DotNet.RateLimiter.Interfaces;
using DotNet.RateLimiter.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DotNet.RateLimiter.ActionFilters
{
    public class RateLimitAttribute : IAsyncActionFilter, IOrderedFilter
    {
        private readonly ILogger<RateLimitAttribute> _logger;
        private readonly IRateLimitService _rateLimitService;
        private readonly IOptions<RateLimitOptions> _options;

        public RateLimitAttribute(ILogger<RateLimitAttribute> logger, IRateLimitService rateLimitService, IOptions<RateLimitOptions> options)
        {
            _logger = logger;
            _rateLimitService = rateLimitService;
            _options = options;
        }

        public int Order { get; set; }
        public int PeriodInSec { get; set; }
        public int Limit { get; set; }
        public string VaryByParams { get; set; }
        public int VaryByParamsPeriodInSec { get; set; }
        public int VaryByParamsLimit { get; set; }
        public RateLimitScope Scope { get; set; }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                if (!_options.Value.EnableRateLimit)
                {
                    await next.Invoke();
                    return;
                }

                var userIp = context.HttpContext.Request.GetUserIp(_options.Value.IpHeaderName).ToString();

                if (_options.Value.IpWhiteList.Contains(userIp))
                {
                    await next.Invoke();
                    return;
                }

                var requestKey = userIp;

                if (!string.IsNullOrWhiteSpace(_options.Value.ClientIdentifier) &&
                    context.HttpContext.Request.Headers.TryGetValue(_options.Value.ClientIdentifier, out var clientId))
                    requestKey = clientId.ToString();

                var controller = context.ActionDescriptor.RouteValues["Controller"];
                var action = context.ActionDescriptor.RouteValues["Action"];

                var rateLimitKey = $"{requestKey}:{controller}";

                if (Scope == RateLimitScope.Action)
                    rateLimitKey = $"{rateLimitKey}:{action}";

                bool hasAccess = await _rateLimitService.HasAccessAsync(rateLimitKey, PeriodInSec, Limit);
                if (hasAccess && !string.IsNullOrWhiteSpace(VaryByParams))
                {
                    var parameters = VaryByParams.Split(',');

                    var paramsKey = "";

                    foreach (var parameter in parameters)
                    {
                        if (context.ActionArguments.ContainsKey(parameter))
                            paramsKey += $"{context.ActionArguments[parameter]}:";
                    }

                    if (!string.IsNullOrWhiteSpace(paramsKey) &&
                        !await _rateLimitService.HasAccessAsync($"{rateLimitKey}:{paramsKey}", VaryByParamsPeriodInSec, VaryByParamsLimit))
                        hasAccess = false;//Rate limit exceeded
                }

                if (!hasAccess)
                {
                    context.HttpContext.Response.StatusCode = _options.Value.HttpStatusCode;

                    context.HttpContext.Response.ContentType = "application/json";

                    var response = new RateLimitResponse()
                    {
                        Code = _options.Value.HttpStatusCode,
                        Message = _options.Value.ErrorMessage
                    };

                    await context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(response));
                }
                else
                    await next.Invoke();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, e.Message);
                await next.Invoke();
            }
        }
    }
}