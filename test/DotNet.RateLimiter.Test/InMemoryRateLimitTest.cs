using System;
using DotNet.RateLimiter.ActionFilters;
using DotNet.RateLimiter.Interfaces;
using DotNet.RateLimiter.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit.DependencyInjection;

namespace DotNet.RateLimiter.Test;

[Startup(typeof(Startup))]
public class InMemoryRateLimitTest : BaseRateLimitTest
{
    public InMemoryRateLimitTest(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
    }
}