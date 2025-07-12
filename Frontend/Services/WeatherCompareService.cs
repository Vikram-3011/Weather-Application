using System.Net.Http.Json;
using Frontend.Models;

public class WeatherCompareService
{
    private readonly HttpClient _http;

    public WeatherCompareService(HttpClient http)
    {
        _http = http;
    }

    public async Task<WeatherData[]> CompareCitiesAsync(string city1, string city2)
    {
        // Call the backend API
        var response = await _http.GetFromJsonAsync<CompareWeatherResponse>(
            $"api/WeatherCompare/compare?city1={city1}&city2={city2}");

        if (response == null || response.City1 == null || response.City2 == null)
            return Array.Empty<WeatherData>();

        // Manually map to frontend WeatherData
        return new WeatherData[]
        {
            new WeatherData
            {
                City = response.City1.City,
                Temp = response.City1.Temp,
                Humidity = response.City1.Humidity,
                WindSpeed = response.City1.WindSpeed
            },
            new WeatherData
            {
                City = response.City2.City,
                Temp = response.City2.Temp,
                Humidity = response.City2.Humidity,
                WindSpeed = response.City2.WindSpeed
            }
        };
    }

    // This matches the structure of the backend's JSON response
    private class CompareWeatherResponse
    {
        public WeatherDto City1 { get; set; }
        public WeatherDto City2 { get; set; }
    }

    private class WeatherDto
    {
        public string City { get; set; } = "";
        public int Temp { get; set; }
        public int Humidity { get; set; }
        public double WindSpeed { get; set; }
    }
}
