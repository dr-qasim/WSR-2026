using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantProduction.Api.Common;
using PlantProduction.Api.Data;
using PlantProduction.Api.Security;

namespace PlantProduction.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    PlantProductionDbContext dbContext,
    JwtTokenService jwtTokenService,
    IConfiguration configuration) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(ApiResponse.Fail("Логин и пароль обязательны."));
        }

        var normalizedLogin = request.Login.Trim().ToLower();

        var user = await dbContext.AppUsers
            .AsNoTracking()
            .Where(x => x.Login.ToLower() == normalizedLogin)
            .Select(x => new
            {
                x.Id,
                x.Login,
                x.FullName,
                x.IsActive,
                RoleCode = x.UserRole.Code,
                RoleName = x.UserRole.Name,
                DepartmentCode = x.Department.Code,
                DepartmentName = x.Department.Name
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return Unauthorized(ApiResponse.Fail("Неверный логин или пароль."));
        }

        if (!user.IsActive)
        {
            return Unauthorized(ApiResponse.Fail("Пользователь отключен."));
        }

        var demoPassword = configuration["Auth:DemoPassword"] ?? "12345";
        if (!string.Equals(request.Password, demoPassword, StringComparison.Ordinal))
        {
            return Unauthorized(ApiResponse.Fail("Неверный логин или пароль."));
        }

        var response = new LoginResponse(
            user.Id,
            user.Login,
            user.FullName,
            user.RoleCode,
            user.RoleName,
            user.DepartmentCode,
            user.DepartmentName,
            jwtTokenService.CreateToken(
                user.Id,
                user.Login,
                user.FullName,
                user.RoleCode,
                user.DepartmentCode));

        return Ok(ApiResponse<LoginResponse>.Ok(response, "Авторизация выполнена."));
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var model = new CurrentUserResponse(
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            User.Identity?.Name ?? string.Empty,
            User.FindFirstValue("full_name") ?? string.Empty,
            User.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
            User.FindFirstValue("department") ?? string.Empty);

        return Ok(ApiResponse<CurrentUserResponse>.Ok(model));
    }

    public sealed record LoginRequest(string Login, string Password);

    public sealed record LoginResponse(
        int Id,
        string Login,
        string FullName,
        string RoleCode,
        string RoleName,
        string DepartmentCode,
        string DepartmentName,
        string Token);

    public sealed record CurrentUserResponse(
        string UserId,
        string Login,
        string FullName,
        string RoleCode,
        string DepartmentCode);
}
