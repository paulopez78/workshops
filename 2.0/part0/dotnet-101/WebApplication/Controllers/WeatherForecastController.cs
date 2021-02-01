using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private          int                                _numberOfResults;
        private          SummariesProvider                  _summariesProvider;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration, SummariesProvider summariesProvider)
        {
            _logger            = logger;
            _numberOfResults   = configuration.GetValue("NumberOfResults", 10);
            _summariesProvider = summariesProvider;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            _logger.LogError("HELLO WORLD!!");
            var summaries = _summariesProvider.GetSummaries();
            var rng       = new Random();
            return Enumerable.Range(1, _numberOfResults).Select(index => new WeatherForecast
                {
                    Date         = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary      = summaries[rng.Next(summaries.Length)]
                })
                .ToArray();
        }
    }

    public class SummariesProvider
    {
        public string[] GetSummaries() => new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
    }
}