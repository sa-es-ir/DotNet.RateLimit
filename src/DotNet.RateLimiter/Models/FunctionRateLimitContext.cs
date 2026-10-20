using System.Collections.Generic;

namespace DotNet.RateLimiter.Models;

public class FunctionRateLimitContext
{
    public string Method { get; set; }
    public string FunctionName { get; set; }
    public string UserIp { get; set; }
    public string ClientId { get; set; }
    public IDictionary<string, object> RouteValues { get; set; }
    public IDictionary<string, IReadOnlyList<string>> QueryValues { get; set; }
}
