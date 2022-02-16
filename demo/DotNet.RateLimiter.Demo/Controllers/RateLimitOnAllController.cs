using DotNet.RateLimiter.ActionFilters;
using Microsoft.AspNetCore.Mvc;

namespace DotNet.RateLimiter.Demo.Controllers
{
    [ApiController]
    [Route("rate-limit-on-controller")]
    [RateLimit(Limit = 3, PeriodInSec = 60, Scope = RateLimitScope.Controller)]//set Scope to rate limit on all actions
    public class RateLimitOnAllController : ControllerBase
    {
        private static readonly string[] Summaries = {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet("")]
        [IgnoreRateLimit]//ignore rate limit for this action
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
                .ToArray();
        }


        [HttpGet("by-route/{id}")]
        public IEnumerable<WeatherForecast> GetByRoute(int id)
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
                .ToArray();
        }

        [HttpGet("by-query/{id}")]
        public IEnumerable<WeatherForecast> GetByQuery(int id, string name, [FromQuery] List<string> family)
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}