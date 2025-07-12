using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

[ApiController]
[Route("api/[controller]")]
public class LocationController : ControllerBase
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly string _apiKey;

    public LocationController(IHttpClientFactory clientFactory, IOptions<OpenCageSettings> options)
    {
        _clientFactory = clientFactory;
        _apiKey = options.Value.ApiKey;
    }

    [HttpPost]
    public async Task<IActionResult> GetAddress([FromBody] LocationDto location)
    {
        var client = _clientFactory.CreateClient();
        var url = $"https://api.opencagedata.com/geocode/v1/json?q={location.Latitude}+{location.Longitude}&key={_apiKey}";

        try
        {
            var result = await client.GetFromJsonAsync<OpenCageResponse>(url);

            if (result?.Results?.Any() == true)
            {
                var c = result.Results.First().Components;

                string address = $"{c.Road}, {c.Town ?? c.Village}, {c.City}, {c.County}, {c.State}, {c.Country}";
                return Ok(address);
            }

            return NotFound("Address not found.");
        }
        catch
        {
            return StatusCode(500, "Failed to get address.");
        }
    }

    public class OpenCageResponse
    {
        public List<Result> Results { get; set; } = new();

        public class Result
        {
            public Components Components { get; set; } = new();
        }

        public class Components
        {
            public string? Road { get; set; }
            public string? Town { get; set; }
            public string? Village { get; set; }
            public string? City { get; set; }
            public string? County { get; set; }
            public string? State { get; set; }
            public string? Country { get; set; }
        }
    }

    public class OpenCageSettings
    {
        public string ApiKey { get; set; }
    }

    public class LocationDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Accuracy { get; set; }
    }

}
