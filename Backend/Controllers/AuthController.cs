using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Supabase;
using Supabase.Gotrue;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly Supabase.Client _supabaseClient;

    public AuthController(IOptions<SupabaseConfig> config)
    {
        var options = new Supabase.SupabaseOptions { AutoConnectRealtime = true };

        if (string.IsNullOrEmpty(config.Value.Url) || string.IsNullOrEmpty(config.Value.ApiKey))
        {
            throw new ArgumentNullException("Supabase URL or API Key is missing from configuration.");
        }

        _supabaseClient = new Supabase.Client(config.Value.Url, config.Value.ApiKey, options);
    }

    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] AuthRequest request)
    {
        try
        {
            var result = await _supabaseClient.Auth.SignUp(request.Email, request.Password);
            if (result?.User != null)
                return Ok("Signup successful, please verify your email.");

            return BadRequest("Signup failed.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    [HttpPost("signin")]
    public async Task<IActionResult> SignIn([FromBody] AuthRequest request)
    {
        try
        {
            var result = await _supabaseClient.Auth.SignIn(request.Email, request.Password);
            if (result?.User == null)
                return Unauthorized("Invalid login credentials.");

            if (!result.User.EmailConfirmedAt.HasValue)
            {
                await _supabaseClient.Auth.SignOut();
                return Unauthorized("Email not verified.");
            }

            return Ok("Login successful.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Login failed: {ex.Message}");
        }
    }

    [HttpPost("signout")]
    public async Task<IActionResult> SignOut()
    {
        await _supabaseClient.Auth.SignOut();
        return Ok("Signed out successfully.");
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            // Re-authenticate with current password
            var session = await _supabaseClient.Auth.SignIn(request.Email, request.CurrentPassword);
            if (session?.User == null)
            {
                return BadRequest("Invalid current password.");
            }

            // Update password
            await _supabaseClient.Auth.Update(new UserAttributes
            {
                Password = request.NewPassword
            });

            return Ok("Password updated successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to change password: {ex.Message}");
        }
    }

    public class AuthRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}


public class SupabaseConfig
{
    public string Url { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
