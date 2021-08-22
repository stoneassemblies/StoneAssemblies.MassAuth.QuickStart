namespace StoneAssemblies.MassAuth.QuickStart.Services.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    using StoneAssemblies.MassAuth.QuickStart.Messages;
    using StoneAssemblies.MassAuth.Services.Attributes;

    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries =
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [AuthorizeByRule]
        public IEnumerable<WeatherForecast> Get([FromQuery] WeatherForecastRequestMessage weatherForecastRequestMessage)
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(
                index => new WeatherForecast
                             {
                                 Date = weatherForecastRequestMessage.StartDate.AddDays(index),
                                 TemperatureC = rng.Next(-20, 55),
                                 Summary = Summaries[rng.Next(Summaries.Length)]
                             }).ToArray();
        }
    }
}