using DotNet.RateLimiter.ActionFilters;
using Microsoft.AspNetCore.Mvc;

namespace DotNet.RateLimiter.Demo.Controllers
{
    [ApiController]
    [Route("rate-limit-on-controller")]
    //if set Scope to Controller to rate limit on all actions no matter which actions call
    //the default value is Action means this rate limit check for each action separately
    [RateLimit(Limit = 3, PeriodInSec = 60, Scope = RateLimitScope.Controller)]
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
                Temperature = Random.Shared.Next(-20, 55),
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
                Temperature = Random.Shared.Next(-20, 55),
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
                Temperature = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }



        [HttpPut]
        public IActionResult Update(WeatherForecast weatherForecast)
        {
            return Ok();
        }
    }
}