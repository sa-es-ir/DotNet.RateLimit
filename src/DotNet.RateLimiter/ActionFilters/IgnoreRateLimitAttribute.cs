using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DotNet.RateLimiter.ActionFilters
{
    /// <summary>
    /// Use for ignoring rate limit
    /// </summary>
    public class IgnoreRateLimitAttribute : Attribute, IIgnoreRateLimitFilter { }

    public interface IIgnoreRateLimitFilter : IFilterMetadata { }
}