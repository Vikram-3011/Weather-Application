using Amazon.Runtime.Internal.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using WeatherApi.Services;

namespace WeatherApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string apiKey = "596a1e71b480c1efbdbdd4da71b6f8da"; // Replace this with your key
        private readonly string _apiKey = "b9693035c52f49d390164524240110"; // backend only
        private readonly IMemoryCache _cache;
        public WeatherController(IHttpClientFactory factory, IMemoryCache cache)
        {
            _httpClient = factory.CreateClient();
            _cache = cache;
        }
      

        [HttpGet("forecast")]
        public async Task<IActionResult> GetWeatherForecast(string city)
        {
            var cacheKey = $"forecast_{city.ToLower()}";

            if (_cache.TryGetValue(cacheKey, out List<DailyForecast> cachedForecast))
            {
                return Ok(cachedForecast);
            }

            var apiUrl = $"https://api.openweathermap.org/data/2.5/forecast?q={city}&units=metric&appid={apiKey}";

            try
            {
                var response = await _httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("Failed to fetch weather data.");
                }

                var json = await response.Content.ReadAsStringAsync();
                var forecastResponse = JsonSerializer.Deserialize<ForecastResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (forecastResponse?.List == null || !forecastResponse.List.Any())
                {
                    return NotFound("No forecast data available.");
                }

                // Group forecasts by date (ignore time)
                var groupedForecasts = forecastResponse.List
                    .GroupBy(f => DateTimeOffset.FromUnixTimeSeconds(f.Dt).Date)
                    .Take(5) // Take first 5 days
                    .Select(group =>
                    {
                        var dayForecasts = group.ToList();
                        var first = dayForecasts.First();

                        return new DailyForecast
                        {
                            Date = group.Key,
                            TempMin = dayForecasts.Min(f => f.Main.TempMin),
                            TempMax = dayForecasts.Max(f => f.Main.TempMax),
                            FeelsLike = (float)dayForecasts.Average(f => f.Main.FeelsLike),
                            Description = first.Weather.FirstOrDefault()?.Description ?? "",
                            Icon = first.Weather.FirstOrDefault()?.Icon ?? "",
                            Humidity = (int)dayForecasts.Average(f => f.Main.Humidity),
                            Pressure = (int)dayForecasts.Average(f => f.Main.Pressure),
                            WindSpeed = (float)dayForecasts.Average(f => f.Wind.Speed)
                        };
                    })
                    .ToList();

                // ✅ Store in cache for 10 minutes
                _cache.Set(cacheKey, groupedForecasts, TimeSpan.FromMinutes(10));

                return Ok(groupedForecasts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        [HttpGet("topcities")]
        public async Task<IActionResult> GetTopCityForecasts()
        {
            const string cacheKey = "TopCityForecasts";

            if (_cache.TryGetValue(cacheKey, out List<CityWeatherForecastDto> cachedForecasts))
            {
                return Ok(cachedForecasts); // ✅ Return cached result
            }

            var topCities = new List<string> { "Delhi", "Mumbai", "Chennai", "Hyderabad", "Kolkata" };
            var result = new List<CityWeatherForecastDto>();

            foreach (var city in topCities)
            {
                var apiUrl = $"https://api.openweathermap.org/data/2.5/forecast?q={city}&units=metric&appid={apiKey}";

                try
                {
                    var response = await _httpClient.GetAsync(apiUrl);
                    if (!response.IsSuccessStatusCode) continue;

                    var json = await response.Content.ReadAsStringAsync();
                    var forecastResponse = JsonSerializer.Deserialize<ForecastResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var firstForecast = forecastResponse?.List?.FirstOrDefault();
                    if (firstForecast != null)
                    {
                        result.Add(new CityWeatherForecastDto
                        {
                            CityName = city,
                            Country = "India",
                            TempMin = firstForecast.Main.TempMin,
                            TempMax = firstForecast.Main.TempMax,
                            Description = firstForecast.Weather.FirstOrDefault()?.Description ?? "",
                            Icon = firstForecast.Weather.FirstOrDefault()?.Icon ?? ""
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching weather for {city}: {ex.Message}");
                }
            }

            // ✅ Save to cache with 10-minute expiration
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            };

            _cache.Set(cacheKey, result, cacheEntryOptions);
            return Ok(result);
        }

        [HttpGet("forecast-by-coordinates")]
        public async Task<IActionResult> GetWeatherByCoordinates(double lat, double lon)
        {

            string cacheKey = $"weather_{lat}_{lon}";

            if (_cache.TryGetValue(cacheKey, out CityWeather cachedWeather))
            {
                return Ok(cachedWeather);
            }

            var apiUrl = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={apiKey}&units=metric";

            try
            {
                var response = await _httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                    return BadRequest("Failed to fetch weather data.");

                var json = await response.Content.ReadAsStringAsync();
                var weatherResponse = JsonSerializer.Deserialize<CityWeatherResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (weatherResponse != null)
                {
                    var result = new CityWeather
                    {
                        CityName = weatherResponse.Name,
                        Country = weatherResponse.Sys?.Country,
                        TempMin = weatherResponse.Main.Temp,   // use .Temp for both Min and Max
                        Description = weatherResponse.Weather.FirstOrDefault()?.Description ?? ""
                    };

                    // Store in cache for 10 minutes
                    _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

                    return Ok(result);
                }

                return NotFound("Weather data not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }


        [HttpGet("user-alerts")]
        public async Task<IActionResult> GetAlerts([FromQuery] string city)
        {
            try
            {
                var cacheKey = $"alerts_{city.ToLower()}";

                // Try to get from cache first
                if (_cache.TryGetValue(cacheKey, out List<WeatherAlert> cachedAlerts))
                {
                    return Ok(cachedAlerts);
                }

                var url = $"https://api.weatherapi.com/v1/alerts.json?key={_apiKey}&q={city}";
                var response = await _httpClient.GetFromJsonAsync<WeatherApiAlertResponse>(url);

                var alerts = response?.Alerts?.Alert ?? new List<WeatherAlert>();


                // Cache the alerts for 10 minutes
                _cache.Set(cacheKey, alerts, TimeSpan.FromMinutes(10));

                return Ok(alerts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ALERT API] Error: {ex.Message}");
                return StatusCode(500, "Failed to fetch alerts.");
            }
        }


        private async Task<(double Lat, double Lon)?> GetCoordinatesFromCityName(string city)
        {
            var geoUrl = $"http://api.openweathermap.org/geo/1.0/direct?q={city}&limit=1&appid={apiKey}";
            var response = await _httpClient.GetAsync(geoUrl);

            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var geo = JsonSerializer.Deserialize<List<GeoResponse>>(json);

            if (geo == null || geo.Count == 0) return null;

            return (geo[0].Lat, geo[0].Lon);
        }

        private async Task<object?> GetAlertForCoordinates(double lat, double lon)
        {
            var alertUrl = $"https://api.openweathermap.org/data/3.0/onecall?lat={lat}&lon={lon}&appid={apiKey}&exclude=minutely,hourly,daily,current";
            var response = await _httpClient.GetAsync(alertUrl);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OneCallAlertResponse>(json);

            return result?.Alerts?.FirstOrDefault(); // Only take first alert per city
        }

        // Models
        public class ForecastResponse
        {
            [JsonPropertyName("list")]
            public List<Forecast> List { get; set; }
        }

        public class Forecast
        {
            [JsonPropertyName("dt")]
            public long Dt { get; set; }

            [JsonPropertyName("main")]
            public MainData Main { get; set; }

            [JsonPropertyName("weather")]
            public List<WeatherDetail> Weather { get; set; }

            [JsonPropertyName("wind")]
            public Wind Wind { get; set; }
        }

        public class MainData
        {
            [JsonPropertyName("temp_min")]
            public float TempMin { get; set; }

            [JsonPropertyName("temp_max")]
            public float TempMax { get; set; }

            [JsonPropertyName("feels_like")]
            public float FeelsLike { get; set; }

            [JsonPropertyName("pressure")]
            public int Pressure { get; set; }

            [JsonPropertyName("humidity")]
            public int Humidity { get; set; }
        }

        public class WeatherDetail
        {
            [JsonPropertyName("description")]
            public string Description { get; set; }

            [JsonPropertyName("icon")]
            public string Icon { get; set; }
        }

        public class Wind
        {
            [JsonPropertyName("speed")]
            public float Speed { get; set; }
        }

        public class DailyForecast
        {
            public DateTime Date { get; set; }
            public float TempMin { get; set; }
            public float TempMax { get; set; }
            public float FeelsLike { get; set; }
            public string Description { get; set; }
            public float WindSpeed { get; set; }
            public int Humidity { get; set; }
            public int Pressure { get; set; }
            public string Icon { get; set; }
        }

        public class CityWeatherForecastDto
        {
            public string CityName { get; set; }
            public string Country { get; set; }
            public float TempMin { get; set; }
            public float TempMax { get; set; }
            public string Description { get; set; }
            public string Icon { get; set; }
        }

        public class CityWeather
        {
            public string CityName { get; set; }
            public string Country { get; set; }
            public float TempMin { get; set; }
            public float TempMax { get; set; }
            public string Description { get; set; }
        }

        public class CityWeatherResponse
        {
            public Main Main { get; set; }
            public List<WeatherDetail> Weather { get; set; }
            public string Name { get; set; }
            public Sys Sys { get; set; }
        }

        public class Main
        {
            [JsonPropertyName("temp")]
            public float Temp { get; set; }
        }


        public class Sys
        {
            public string Country { get; set; }
        }

    }

   

}
public class GeoResponse
{
    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lon")]
    public double Lon { get; set; }
}

public class OneCallAlertResponse
{
    [JsonPropertyName("alerts")]
    public List<WeatherAlert>? Alerts { get; set; }
}
public class WeatherApiAlertResponse
{
    public AlertContainer? Alerts { get; set; }
}
public class WeatherAlert
{
    public string Headline { get; set; } = "";
    public string Severity { get; set; } = "";
    public string Urgency { get; set; } = "";
    public string Areas { get; set; } = "";
    public string Event { get; set; } = "";
    public string Effective { get; set; } = "";
    public string Expires { get; set; } = "";
    public string Desc { get; set; } = "";
    public string Instruction { get; set; } = "";
}

public class AlertContainer
{
    public List<WeatherAlert>? Alert { get; set; }
}