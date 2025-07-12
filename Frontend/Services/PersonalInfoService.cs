using System.Net.Http.Json;

public class PersonalInfoService
{
    private readonly HttpClient _http;

    public PersonalInfoService(HttpClient http)
    {
        _http = http;
    }

    public async Task<PersonalInfo?> GetUserByEmailAsync(string email)
    {
        try
        {
            return await _http.GetFromJsonAsync<PersonalInfo>($"api/PersonalInfo/{email}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> SavePersonalInfoAsync(PersonalInfo info)
    {
        var response = await _http.PostAsJsonAsync("api/PersonalInfo", info);
        return response.IsSuccessStatusCode;
    }

}
public class PersonalInfo
{
    public string Id { get; set; } = string.Empty; // Optional
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}
