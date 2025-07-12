using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PersonalInfoController : ControllerBase
{
    private readonly PersonalInfoService _service;

    public PersonalInfoController(PersonalInfoService service)
    {
        _service = service;
    }

    [HttpGet("{email}")]
    public async Task<ActionResult<PersonalInfo>> Get(string email)
    {
        if (string.IsNullOrEmpty(email))
            return BadRequest("Email is required");

        var result = await _service.GetByEmailAsync(email);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] PersonalInfo info)
    {
        if (info == null || string.IsNullOrWhiteSpace(info.Email))
            return BadRequest("Invalid user info");

        var success = await _service.UpsertAsync(info);
        return success ? Ok() : StatusCode(500, "Failed to save");
    }
}
