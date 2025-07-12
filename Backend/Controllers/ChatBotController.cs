using BlazorApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using WeatherApi.Models;

[ApiController]
[Route("api/[controller]")]
public class ChatBotController : ControllerBase
{
    private readonly IHttpClientFactory _factory;
    private readonly IConfiguration _config;

    public ChatBotController(IHttpClientFactory factory, IConfiguration config)
    {
        _factory = factory;
        _config = config;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] List<GeminiChatMessage> messages)
    {
        var apiKey = _config["Gemini:ApiKey"];
        var endpoint = $"https://generativelanguage.googleapis.com/v1/models/gemini-1.5-pro:generateContent?key={apiKey}";

        var contentPayload = messages.Select(m => new
        {
            role = m.Role,
            parts = new[] { new { text = m.Content } }
        });

        var requestBody = new { contents = contentPayload };
        var json = JsonSerializer.Serialize(requestBody);
        var client = _factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var response = await client.SendAsync(request);
        var jsonResponse = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode, jsonResponse);

        using var doc = JsonDocument.Parse(jsonResponse);
        var reply = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return Ok(reply ?? "No reply from Gemini.");
    }
}
namespace BlazorApp.Models
{
    public class GeminiChatMessage
    {
        public string Role { get; set; } = "user"; // or "model"
        public string Content { get; set; } = "";
    }
}
