using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace DotNet.RateLimiter.ActionFilters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RateLimit : Attribute, IFilterFactory
    {
        /// <summary>
        /// order execution
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// period of time in seconds for rate limit
        /// </summary>
        public int PeriodInSec { get; set; }

        /// <summary>
        /// limit of requests
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// seek in route parameters and check rate limit for specific params, for multiple parameters separate them by comma (,)
        /// </summary>
        public string RouteParams { get; set; }

        /// <summary>
        /// seek in query string parameters and check rate limit for specific params, for multiple parameters separate them by comma (,)
        /// </summary>
        public string QueryParams { get; set; }

        /// <summary>
        /// seek in body parameters and check rate limit for specific params, for multiple parameters separate them by comma (,)
        /// </summary>
        public string BodyParams { get; set; }

        /// <summary>
        /// if scope set to Controller, the rate limit will work for entire controller no matter which action calls and won't consider action limit, default is Action.
        /// </summary>
        public RateLimitScope Scope { get; set; }

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var filter = serviceProvider.GetRequiredService<RateLimitAttribute>();
            filter.Order = Order;
            filter.PeriodInSec = PeriodInSec;
            filter.Limit = Limit;
            filter.RouteParams = RouteParams;
            filter.QueryParams = QueryParams;
            filter.BodyParams = BodyParams;
            filter.Scope = Scope;

            return filter;
        }

        public bool IsReusable => true;
    }
}