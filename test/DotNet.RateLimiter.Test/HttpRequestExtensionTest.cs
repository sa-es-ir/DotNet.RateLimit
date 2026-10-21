using DotNet.RateLimiter.Extensions;
using Shouldly;
using Xunit;

namespace DotNet.RateLimiter.Test;

public class HttpRequestExtensionTest
{
    [Theory]
    [InlineData("203.0.113.5", "203.0.113.5")]
    [InlineData("203.0.113.5:54321", "203.0.113.5")]
    [InlineData("[2001:db8::1]:443", "2001:db8::1")]
    [InlineData("2001:db8::1", "2001:db8::1")]
    [InlineData("not-an-ip, 203.0.113.5:54321", "203.0.113.5")]
    public void GetIpAddresses_ParsesAddressesWithAndWithoutPort(string header, string expected)
    {
        header.GetIpAddresses()[0].ToString().ShouldBe(expected);
    }
}
