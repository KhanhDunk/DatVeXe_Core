using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private const string ConfigFile = "appsettings.userconfig.json";

    // Admin only: update config
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult SaveConfig([FromBody] JsonElement cfg)
    {
        try
        {
            var json = JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(ConfigFile, json);
            return Ok(new { message = "Saved" });
        }
        catch
        {
            return StatusCode(500, new { message = "Failed to save" });
        }
    }

    // Any authenticated user: read config
    [HttpGet]
    [Authorize]
    public IActionResult GetConfig()
    {
        if (!System.IO.File.Exists(ConfigFile))
        {
            return Ok(new { });
        }

        var json = System.IO.File.ReadAllText(ConfigFile);
        var doc = JsonSerializer.Deserialize<JsonElement>(json);
        return Ok(doc);
    }
}
