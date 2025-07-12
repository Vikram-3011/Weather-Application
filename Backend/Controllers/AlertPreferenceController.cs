using Microsoft.AspNetCore.Mvc;
using WeatherApi.Services;

namespace WeatherApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertPreferenceController : ControllerBase
    {
        private readonly AlertPreferenceService _service;

        public AlertPreferenceController(AlertPreferenceService service)
        {
            _service = service;
        }

        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] AlertPreferenceRequest request)
        {
            await _service.SavePreferencesAsync(request.Email, request.Preferences);
            return Ok();
        }

        [HttpGet("get")]
        public async Task<IActionResult> Get([FromQuery] string email)
        {
            var prefs = await _service.GetPreferencesAsync(email);
            return Ok(prefs);
        }
    }

    public class AlertPreferenceRequest
    {
        public string Email { get; set; } = "";
        public List<string> Preferences { get; set; } = new();
    }
}
