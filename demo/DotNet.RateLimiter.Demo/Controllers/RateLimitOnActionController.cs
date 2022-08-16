using DotNet.RateLimiter.ActionFilters;
using Microsoft.AspNetCore.Mvc;

namespace DotNet.RateLimiter.Demo.Controllers
{
    [ApiController]
    [Route("rate-limit-on-actions")]
    public class RateLimitOnActionController : ControllerBase
    {
        private static readonly string[] Summaries = {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        /// <summary>
        /// rate limit without any parameters
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        [RateLimit(PeriodInSec = 60, Limit = 3)]
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

        /// <summary>
        /// rate limit with route parameters
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("by-route/{id}")]
        [RateLimit(PeriodInSec = 60, Limit = 3, RouteParams = "id")]
        public IEnumerable<WeatherForecast> Get(int id)
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
                .ToArray();
        }

        /// <summary>
        /// rate limit with route and query parameters
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="family"></param>
        /// <returns></returns>
        [HttpGet("by-query/{id}")]
        [RateLimit(PeriodInSec = 60, Limit = 3, RouteParams = "id", QueryParams = "name,family")]
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


        /// <summary>
        /// rate limit with body parameters
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [RateLimit(PeriodInSec = 60, Limit = 3, BodyParams = "temperatureC" )]
        public IActionResult Update(WeatherForecast weatherForecast)
        {
            return Ok();
        }
    }
}