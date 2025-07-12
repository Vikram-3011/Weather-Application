using Microsoft.AspNetCore.Mvc;
using WeatherApi.Models;
using WeatherApi.Services;

namespace WeatherApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FavoriteCitiesController : ControllerBase
    {
        private readonly FavoriteCityService _service;

        public FavoriteCitiesController(FavoriteCityService service)
        {
            _service = service;
        }

        // Add favorite city
        [HttpPost("add")]
        public async Task<IActionResult> AddFavorite([FromBody] AddFavoriteRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.City))
                return BadRequest("Invalid input");

            await _service.AddCityAsync(request.Email, request.City);
            return Ok();
        }

        // Get favorite cities
        [HttpGet("get")]
        public async Task<IActionResult> Get([FromQuery] string email)
        {
            try
            {
                var cities = await _service.GetCitiesAsync(email);
                return Ok(cities);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FavoriteCitiesController] Error: {ex}");
                return StatusCode(500, "Server error occurred.");
            }
        }

        // Remove favorite city
        [HttpDelete("remove")]
        public async Task<IActionResult> Remove([FromQuery] string email, [FromQuery] string city)
        {
            await _service.RemoveCityAsync(email, city);
            return Ok();
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
