using Microsoft.AspNetCore.Mvc;

namespace TasksAppAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public HealthController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpGet("version")]
    public IActionResult Version()
    {
        var versionPath = Path.Combine(_env.ContentRootPath, "wwwroot", "app-version.json");
        if (System.IO.File.Exists(versionPath))
        {
            var json = System.IO.File.ReadAllText(versionPath);
            return Content(json, "application/json");
        }

        return Ok(new
        {
            commit = "unknown",
            builtAt = (string?)null,
            features = Array.Empty<string>()
        });
    }
}
