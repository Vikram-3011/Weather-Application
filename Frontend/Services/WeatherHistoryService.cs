using WeatherApi.Shared;
using System.Net.Http.Json;

namespace Frontend.Services
{
    public class WeatherHistoryService
    {
        private readonly HttpClient _http;

        public WeatherHistoryService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<WeatherData>> GetWeatherHistory(string city, int days)
        {
            return await _http.GetFromJsonAsync<List<WeatherData>>($"api/WeatherHistory/{city}/{days}");
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
