using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantProduction.Api.Common;
using PlantProduction.Api.Model;

namespace PlantProduction.Api.Controllers;

[ApiController]
[Route("api/system")]
public sealed class SystemController(PlantProductionScaffoldDbContext dbContext) : ControllerBase
{
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
    {
        var databaseReady = await dbContext.Database.CanConnectAsync(cancellationToken);

        return Ok(ApiResponse<SystemStatusResponse>.Ok(new SystemStatusResponse(
            "PlantProduction",
            databaseReady ? "ready" : "database_error",
            databaseReady,
            new[]
            {
                "api",
                "desktop",
                "web",
                "database"
            },
            new[]
            {
                "database",
                "api",
                "desktop",
                "web"
            })));
    }
}

public sealed record SystemStatusResponse(
    string Project,
    string Status,
    bool DatabaseReady,
    string[] Modules,
    string[] Priorities);
