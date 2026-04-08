using Microsoft.AspNetCore.Mvc;

namespace PlantProduction.Api.Controllers;

[ApiController]
[Route("api/system")]
public sealed class SystemController : ControllerBase
{
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            project = "PlantProduction",
            status = "ready",
            modules = new[]
            {
                "api",
                "desktop",
                "web",
                "database"
            },
            priorities = new[]
            {
                "database",
                "api",
                "desktop",
                "web"
            }
        });
    }
}
