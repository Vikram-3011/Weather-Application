using System.Net.Http.Json;
using Frontend.Services;
using Frontend.Models; // ✅ ADD THIS LINE

public class AuthService
{
    private readonly HttpClient _http;
    private readonly UserStateManager _userState;

    public AuthService(HttpClient http, UserStateManager userState)
    {
        _http = http;
        _userState = userState;
    }

    public async Task<string?> SignUp(string email, string password)
    {
        var res = await _http.PostAsJsonAsync("api/auth/signup", new { Email = email, Password = password });

        if (res.IsSuccessStatusCode)
        {
            _userState.SetUser(new User(email)); // ✅ Now 'User' will be recognized
            Console.WriteLine("User logged in: " + email);

            return null;
        }

        return await res.Content.ReadAsStringAsync();
    }

    public async Task<string?> SignIn(string email, string password)
    {
        var res = await _http.PostAsJsonAsync("api/auth/signin", new { Email = email, Password = password });

        if (res.IsSuccessStatusCode)
        {
            _userState.SetUser(new User(email)); // ✅ Fix works here too
            return null;
        }

        return await res.Content.ReadAsStringAsync();
    }

    public async Task SignOut()
    {
        await _http.PostAsync("api/auth/signout", null);
        _userState.ClearUser();
    }
    public async Task<string?> ChangePasswordAsync(string email, string currentPassword, string newPassword)
    {
        var request = new
        {
            Email = email,
            CurrentPassword = currentPassword,
            NewPassword = newPassword
        };

        var response = await _http.PostAsJsonAsync("api/auth/change-password", request);

        if (response.IsSuccessStatusCode)
        {
            return null; // success
        }

        var error = await response.Content.ReadAsStringAsync();
        return error;
    }


}
