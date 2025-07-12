using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Backend.Configuration;
using System.Text.Json;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherCompareController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly WeatherSettings _settings;

        public WeatherCompareController(IHttpClientFactory httpClientFactory, IOptions<WeatherSettings> settings)
        {
            _httpClient = httpClientFactory.CreateClient();
            _settings = settings.Value;
        }

        [HttpGet("compare")]
        public async Task<IActionResult> CompareCities([FromQuery] string city1, [FromQuery] string city2)
        {
            if (string.IsNullOrWhiteSpace(city1) || string.IsNullOrWhiteSpace(city2))
                return BadRequest("City names are required.");

            var result1 = await FetchWeather(city1);
            var result2 = await FetchWeather(city2);

            if (result1 == null || result2 == null)
                return NotFound("Weather data not found for one or both cities.");

            return Ok(new { city1 = result1, city2 = result2 });
        }

        private async Task<WeatherData?> FetchWeather(string city)
        {
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={_settings.ApiKey}&units=metric";

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                var weather = JsonSerializer.Deserialize<OpenWeatherResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return weather == null ? null : new WeatherData
                {
                    City = city,
                    Temp = (int)weather.Main.Temp,
                    Humidity = (int)weather.Main.Humidity,
                    WindSpeed = weather.Wind.Speed
                };
            }
            catch
            {
                return null;
            }
        }

        // Data classes
        public class WeatherData
        {
            public string City { get; set; }
            public int Temp { get; set; }
            public int Humidity { get; set; }
            public double WindSpeed { get; set; }
        }

        public class OpenWeatherResponse
        {
            public Main Main { get; set; }
            public Wind Wind { get; set; }
        }

        public class Main
        {
            public double Temp { get; set; }
            public double Humidity { get; set; }
        }

        public class Wind
        {
            public double Speed { get; set; }
        }
    }
}

namespace Backend.Configuration
{
    public class WeatherSettings
    {
        public string ApiKey { get; set; } = string.Empty;
    }
}
