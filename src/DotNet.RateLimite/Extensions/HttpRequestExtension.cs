using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace DotNet.RateLimit.Extensions
{
    public static class HttpRequestExtension
    {
        public static string GetUserAgent(this HttpRequest request)
        {
            return request.Headers["User-Agent"].ToString();
        }

        public static IPAddress GetUserIp(this HttpRequest request)
        {
            if (request.Headers["X-Forwarded-For"].FirstOrDefault() != null)
            {
                return request.Headers["X-Forwarded-For"].FirstOrDefault().GetIpAddresses().FirstOrDefault();
            }

            return request.HttpContext.Connection.RemoteIpAddress.MapToIPv4();
        }

        /// <summary>
        /// get ip addresses
        /// </summary>
        /// <param name="ips"></param>
        /// <param name="separator">ips separator(default is ',')</param>
        /// <returns></returns>
        public static List<IPAddress> GetIpAddresses(this string ips, string separator = ",")
        {
            var ipAddresses = new List<IPAddress>();
            var ipList = ips.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);

            if (ipList.Length == 0)
                return default;

            foreach (var ip in ipList)
            {
                if (IPAddress.TryParse(ip.Trim(), out var ipAddress))
                    ipAddresses.Add(ipAddress);
            }

            return ipAddresses;
        }

    }
}