using DotNet.RateLimiter.ActionFilters;
using Microsoft.AspNetCore.Mvc;

namespace DotNet.RateLimiter.Demo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet("GetWeatherForecast/{id}")]
        [RateLimit(PeriodInSec = 60, Limit = 5, RouteParams = "id", QueryParams = "name,family")]
        public IEnumerable<WeatherForecast> Get(int id, string name, [FromQuery] List<string> family)
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