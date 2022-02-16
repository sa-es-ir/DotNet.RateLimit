using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DotNet.RateLimiter.ActionFilters
{
    /// <summary>
    /// using for ignore rate limit
    /// </summary>
    public class IgnoreRateLimitAttribute : Attribute, IIgnoreRateLimitFilter { }

    public interface IIgnoreRateLimitFilter : IFilterMetadata { }
}