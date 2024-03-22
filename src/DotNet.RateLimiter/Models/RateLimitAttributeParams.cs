using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.RateLimiter.Models;

public class RateLimitAttributeParams
{
    public int PeriodInSec { get; set; }
    public int Limit { get; set; }
    public string RouteParams { get; set; }
    public string QueryParams { get; set; }
    public string BodyParams { get; set; }
    public RateLimitScope Scope { get; set; }
}

public class RateLimitEndPointParams
{
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
}
