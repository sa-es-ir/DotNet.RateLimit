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
        /// by default rate limit work on IP address but if userIdentifier set it will look at HttpContext.Items[UserIdentifier]
        /// </summary>
        public string UserIdentifier { get; set; }

        /// <summary>
        /// period of time in seconds for rate limit
        /// </summary>
        public int PeriodInSec { get; set; }
        public int Limit { get; set; }
        public string VaryByParams { get; set; }
        public int VaryByParamsPeriodInSec { get; set; }
        public int VaryByParamsLimit { get; set; }

        /// <summary>
        /// if scope set to Controller, the rate limit will set to all actions and won't consider action limit, default is Action.
        /// </summary>
        public RateLimitScope Scope { get; set; } = RateLimitScope.Action;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var filter = serviceProvider.GetRequiredService<RateLimitAttribute>();

            filter.Order = Order;
            filter.UserIdentifier = UserIdentifier;
            filter.PeriodInSec = PeriodInSec;
            filter.Limit = Limit;
            filter.VaryByParams = VaryByParams;
            filter.VaryByParamsPeriodInSec = VaryByParamsPeriodInSec;
            filter.VaryByParamsLimit = VaryByParamsLimit;

            return filter;
        }

        public bool IsReusable => true;
    }
}