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
