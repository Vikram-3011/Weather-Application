using System.Net.Http.Json;
using Frontend.Models;

namespace Frontend.Services
{
    public class WeatherAlertService
    {
        private readonly HttpClient _http;

        public WeatherAlertService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<WeatherAlert>> GetAllAlerts(string email, List<string> cities)
        {
            var allAlerts = new List<WeatherAlert>();

            foreach (var city in cities)
            {
                try
                {
                    var alerts = await _http.GetFromJsonAsync<List<WeatherAlert>>($"api/Weather/user-alerts?city={city}");
                    if (alerts != null)
                        allAlerts.AddRange(alerts);
                }
                catch
                {
                    // Log if needed
                }
            }

            return allAlerts;
        }

    }
}
