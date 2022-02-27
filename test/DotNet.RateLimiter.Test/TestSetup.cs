using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DotNet.RateLimiter.Test;

public class TestSetup
{
    protected string? TestMessage;
    protected TestSetup()
    {
        var fa = new WebApplicationFactory<StartupBase>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.Sources.Clear();
                config.AddJsonFile("appsettings.json");
            });



        });
        TestMessage = fa.Services.GetRequiredService<IConfiguration>()["RateLimitOption:ErrorMessage"];
    }
}