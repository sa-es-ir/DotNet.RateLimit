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

    public static IEnumerable<object[]> OkTestDataWithRouteParams =>
        new List<object[]>
        {
            new object[] { 1, 60, new Dictionary<string, object?>()
            {
                {"id","20"},
                {"name","rate-limit"},
            }, HttpStatusCode.OK }
        };

    public static IEnumerable<object[]> TooManyRequestTestDataWithRouteParams =>
        new List<object[]>
        {
            new object[] { 1, 60, new Dictionary<string, object?>()
            {
                {"id","20"},
                {"name","rate-limit"}
            }, HttpStatusCode.TooManyRequests }
        };

    public static IEnumerable<object[]> OkTestDataWithRouteAndQueryParams =>
        new List<object[]>
        {
            new object[] { 1, 60, new Dictionary<string, object?>()
                {
                    {"id","20"},
                    {"name","rate-limit"}
                },
                new Dictionary<string, object>()
                {
                    {"q1","query1"},
                    {"q2","query2"}
                }
                , HttpStatusCode.OK }
        };

    public static IEnumerable<object[]> TooManyRequestTestDataWithRouteAndQueryParams =>
        new List<object[]>
        {
            new object[] { 1, 60, new Dictionary<string, object?>()
                {
                    {"id","20"},
                    {"name","rate-limit"},
                },
                new Dictionary<string, object>()
                {
                    {"q1","query1"},
                    {"q2","query2"}
                },
                HttpStatusCode.TooManyRequests }
        };

    public static IEnumerable<object[]> OkTestDataWithBodyParams =>
        new List<object[]>
        {
            new object[] { 1, 60, new Dictionary<string, object?>()
            {
                {"id","20"},
                {"name","rate-limit"},
            }, HttpStatusCode.OK }
        };


    public static IEnumerable<object[]> TooManyRequestTestDataWithBodyParams =>
        new List<object[]>
        {
            new object[] { 1, 60, new Dictionary<string, object?>()
            {
                {"id","20"},
                {"name","rate-limit"}
            }, HttpStatusCode.TooManyRequests }
        };
}