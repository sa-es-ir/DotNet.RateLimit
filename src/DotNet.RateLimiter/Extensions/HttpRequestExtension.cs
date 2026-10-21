using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DotNet.RateLimiter.Extensions
{
    public static class HttpRequestExtension
    {
        public static IPAddress GetUserIp(this HttpRequest request, string headerName)
        {
            if (request.Headers[headerName].FirstOrDefault() != null)
            {
                return request.Headers[headerName].FirstOrDefault().GetIpAddresses().FirstOrDefault();
            }

            return request.HttpContext.Connection.RemoteIpAddress;
        }

        public static List<IPAddress> GetIpAddresses(this string ips, string separator = ",")
        {
            var ipAddresses = new List<IPAddress>();
            var ipList = ips.Split([separator], StringSplitOptions.RemoveEmptyEntries);

            if (ipList.Length == 0)
                return default;

            foreach (var ip in ipList)
            {
                var value = ip.Trim();
                if (IPAddress.TryParse(value, out var ipAddress))
                {
                    ipAddresses.Add(ipAddress);
                    continue;
                }

                //IPv4 with port, e.g. "203.0.113.5:54321" as written by Azure App Service; IPv6 with port ("[::1]:443") already parses above
                var colon = value.IndexOf(':');
                if (colon > 0 && colon == value.LastIndexOf(':') && IPAddress.TryParse(value.Substring(0, colon), out ipAddress))
                    ipAddresses.Add(ipAddress);
            }

            return ipAddresses;
        }
    }
}