using System.Net.Http.Json;
using Frontend.Models;

namespace Frontend.Services
{
    public class FavoriteCityService
    {
        private readonly HttpClient _http;

        public FavoriteCityService(HttpClient http)
        {
            _http = http;
        }

        public async Task AddFavoriteCity(string email, string city)
        {
            var request = new AddFavoriteRequest { Email = email, City = city };
            await _http.PostAsJsonAsync("api/FavoriteCities/add", request);
        }

        public async Task<List<string>> GetFavoriteCities(string email)
        {
            try
            {
                var response = await _http.GetAsync($"api/FavoriteCities/get?email={email}");
                response.EnsureSuccessStatusCode();

                var cities = await response.Content.ReadFromJsonAsync<List<string>>();
                return cities ?? new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Frontend ERROR] Failed to fetch favorite cities: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task RemoveFavoriteCity(string email, string city)
        {
            await _http.DeleteAsync($"api/FavoriteCities/remove?email={email}&city={city}");
        }
    }
}
namespace WeatherApi.Models
{
    public class AddFavoriteRequest
    {
        public string Email { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }
}
