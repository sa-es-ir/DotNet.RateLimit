﻿using System;
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

    public static IEnumerable<object[]> TrueTestDataWithNoParams =>
       new List<object[]>
       {
            new object[] { 1, 60, true }
       };

    public static IEnumerable<object[]> FalseTestDataWithNoParams =>
      new List<object[]>
      {
            new object[] { 1, 60, false }
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

    public static IEnumerable<object[]> TrueTestDataWithRouteParams =>
        new List<object[]>
        {
            new object[] { 1, 60, new Dictionary<string, object?>()
            {
                {"id","20"},
                {"name","rate-limit"},
            }, true }
        };

    public static IEnumerable<object[]> FalseTestDataWithRouteParams =>
        new List<object[]>
        {
            new object[] { 1, 60, new Dictionary<string, object?>()
            {
                {"id","20"},
                {"name","rate-limit"},
            }, false }
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

    public static IEnumerable<object[]> TrueTestDataWithRouteAndQueryParams =>
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
                , true }
       };

    public static IEnumerable<object[]> FalseTestDataWithRouteAndQueryParams =>
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
                , false }
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
                {"Model", new {Id = 20, Name = "rate-limit"} }
            }, HttpStatusCode.OK }
        };


    public static IEnumerable<object[]> TooManyRequestTestDataWithBodyParams =>
        new List<object[]>
        {
            new object[] { 1, 60, new Dictionary<string, object?>()
            {
                {"Model", new {Id = 20, Name = "rate-limit"} }
            }, HttpStatusCode.TooManyRequests }
        };
}