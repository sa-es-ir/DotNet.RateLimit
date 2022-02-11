using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace DotNet.RateLimiter.ActionFilters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RateLimit : Attribute, IFilterFactory
    {
        public int Order { get; set; }
        public string UserIdentifier { get; set; }
        public int PeriodInSec { get; set; }
        public int Limit { get; set; }
        public string VaryByParams { get; set; }
        public int VaryByParamsPeriodInSec { get; set; }
        public int VaryByParamsLimit { get; set; }

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