using System;
using DotNet.RateLimiter.EndPointFilters;
using DotNet.RateLimiter.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace DotNet.RateLimiter.Extensions
{
    public static class EndPointExtensions
    {
        public static TBuilder WithRateLimiter<TBuilder>(this TBuilder builder, Action<RateLimitEndPointParams> options)
            where TBuilder : IEndpointConventionBuilder
        {
            // We call `CreateFactory` twice here since the `CreateFactory` API does not support optional arguments.
            // See https://github.com/dotnet/runtime/issues/67309 for more info.
            ObjectFactory filterFactory;
            try
            {
                filterFactory = ActivatorUtilities.CreateFactory(typeof(RateLimitEndPointFilter), new[] { typeof(EndpointFilterFactoryContext) });
            }
            catch (InvalidOperationException)
            {
                filterFactory = ActivatorUtilities.CreateFactory(typeof(RateLimitEndPointFilter), Type.EmptyTypes);
            }

            builder.AddEndpointFilterFactory((routeHandlerContext, next) =>
            {
                var invokeArguments = new[] { routeHandlerContext };
                return (context) =>
                {
                    var opt = new RateLimitEndPointParams();
                    options(opt);
                    context.HttpContext.Features.Set(opt);

                    var filter = (IEndpointFilter)filterFactory.Invoke(context.HttpContext.RequestServices, invokeArguments);
                    return filter.InvokeAsync(context, next);
                };
            });

            return builder;
        }
    }
}