using System;
using System.Collections.Generic;
using System.Net;

namespace DotNet.RateLimiter.Test;

public class TestDataProvider
{
    public static IEnumerable<object[]> OkTestDataWithNoParams =>
        new List<object[]>
        {
            new object[] { 1, 60, new Dictionary<string, object?>(), HttpStatusCode.OK }
        };

    public static IEnumerable<object[]> TooManyRequestTestDataWithNoParams =>
        new List<object[]>
        {
            new object[] { 1, 60, new Dictionary<string, object?>(), HttpStatusCode.TooManyRequests }
        };
}