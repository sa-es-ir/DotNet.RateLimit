using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net;

namespace DotNet.RateLimiter.Extensions
{
    public static class HttpRequestExtension
    {
        public static IPAddress GetUserIp(this HttpRequest request, string headerName)
        {
            return request.Headers[headerName].FirstOrDefault()?.GetIpAddress() ??
                request.HttpContext.Connection.RemoteIpAddress;
        }

        public static IPAddress GetIpAddress(this string ips, string separator = ",")
        {
            var position = ips.IndexOf(separator);
            if (position >= 0 && IPAddress.TryParse(ips.Substring(0, position + 1).Trim(), out var ipAddress))
            {
                return ipAddress;
            }
            return null;
        }
    }
}