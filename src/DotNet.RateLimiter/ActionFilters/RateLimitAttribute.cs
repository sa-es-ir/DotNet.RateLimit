using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNet.RateLimiter.Extensions;
using DotNet.RateLimiter.Interfaces;
using DotNet.RateLimiter.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotNet.RateLimiter.ActionFilters
{
    public class RateLimitAttribute : IAsyncActionFilter, IOrderedFilter
    {
        private readonly IOptions<RateLimitOptions> _options;
        private readonly IRateLimitCoordinator _rateLimitCoordinator;

        public RateLimitAttribute(IOptions<RateLimitOptions> options,
            IRateLimitCoordinator rateLimitCoordinator)
        {
            _options = options;
            _rateLimitCoordinator = rateLimitCoordinator;
        }

        public int Order { get; set; }

        public RateLimitParams RateLimitParams { get; set; }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var goodToGo = await _rateLimitCoordinator.CheckRateLimitAsync(context, RateLimitParams);

            if (goodToGo)
                await next.Invoke();
            else
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
        }
    }
}