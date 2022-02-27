using FluentAssertions;
using Xunit;

namespace DotNet.RateLimiter.Test;

public class RateLimitTest : TestSetup
{
    [Fact]
    public void FistTest()
    {
        TestMessage.Should().Be("Rate limit Exceeded");
    }
}