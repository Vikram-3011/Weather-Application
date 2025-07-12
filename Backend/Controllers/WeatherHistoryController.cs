using Microsoft.AspNetCore.Mvc;
using WeatherApi.Shared;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace WeatherApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherHistoryController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private const string ApiKey = "b9693035c52f49d390164524240110"; // Your actual API key
        private const string BaseUrl = "https://api.weatherapi.com/v1/history.json";

        public WeatherHistoryController(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient();
        }

        [HttpGet("{city}/{days}")]
        public async Task<ActionResult<List<WeatherData>>> GetHistoricalWeather(string city, int days)
        {
            var data = new List<WeatherData>();

            try
            {
                for (int i = 0; i < days; i++)
                {
                    var date = DateTime.UtcNow.AddDays(-i).ToString("yyyy-MM-dd");
                    var url = $"{BaseUrl}?key={ApiKey}&q={city}&dt={date}";

                    var result = await _httpClient.GetFromJsonAsync<WeatherApiResponse>(url);

                    if (result?.Forecast?.Forecastday?.Count > 0)
                    {
                        var day = result.Forecast.Forecastday[0].Day;

                        data.Add(new WeatherData
                        {
                            City = city,
                            Date = DateTime.UtcNow.AddDays(-i),
                            Temperature = day.AvgTempC,
                            WindSpeed = day.MaxWindKph / 3.6,
                            Humidity = day.AvgHumidity
                        });
                    }
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        // internal types for deserialization
        public class WeatherApiResponse
        {
            public Forecast Forecast { get; set; }
        }

        public class Forecast
        {
            public List<ForecastDay> Forecastday { get; set; }
        }

        public class ForecastDay
        {
            public string Date { get; set; }
            public Day Day { get; set; }
        }

        public class Day
        {
            [JsonPropertyName("avgtemp_c")]
            public double AvgTempC { get; set; }

            [JsonPropertyName("maxwind_kph")]
            public double MaxWindKph { get; set; }

            [JsonPropertyName("avghumidity")]
            public int AvgHumidity { get; set; }
        }



    }

}

namespace WeatherApi.Shared
{
    public class WeatherData
    {
        public string City { get; set; } = "";
        public DateTime Date { get; set; }
        public double Temperature { get; set; }
        public double WindSpeed { get; set; }
        public int Humidity { get; set; }
    }
}
