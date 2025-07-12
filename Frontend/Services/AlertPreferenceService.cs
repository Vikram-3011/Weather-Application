using System.Net.Http.Json;

namespace Frontend.Services
{
    public class AlertPreferenceService
    {
        private readonly HttpClient _http;

        public AlertPreferenceService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<string>> GetPreferencesAsync(string email)
        {
            return await _http.GetFromJsonAsync<List<string>>($"api/AlertPreference/get?email={email}") ?? new();
        }

        public async Task SavePreferencesAsync(string email, List<string> preferences)
        {
            var payload = new { Email = email, Preferences = preferences };
            await _http.PostAsJsonAsync("api/AlertPreference/save", payload);
        }
    }
}
