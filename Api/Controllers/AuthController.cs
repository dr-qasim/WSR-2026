using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using PlantProduction.Api.Common;
using PlantProduction.Api.Security;

namespace PlantProduction.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    NpgsqlDataSource dataSource,
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

        const string sql = """
            SELECT
                u.id,
                u.login,
                u.full_name,
                u.is_active,
                r.code AS role_code,
                r.name AS role_name,
                d.code AS department_code,
                d.name AS department_name
            FROM app_users u
            JOIN user_roles r ON r.id = u.user_role_id
            JOIN departments d ON d.id = u.department_id
            WHERE LOWER(u.login) = LOWER(@login)
            LIMIT 1;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("login", request.Login.Trim());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return Unauthorized(ApiResponse.Fail("Неверный логин или пароль."));
        }

        if (!reader.GetBoolean("is_active"))
        {
            return Unauthorized(ApiResponse.Fail("Пользователь отключен."));
        }

        var demoPassword = configuration["Auth:DemoPassword"] ?? "12345";
        if (!string.Equals(request.Password, demoPassword, StringComparison.Ordinal))
        {
            return Unauthorized(ApiResponse.Fail("Неверный логин или пароль."));
        }

        var response = new LoginResponse(
            reader.GetInt32("id"),
            reader.GetString("login"),
            reader.GetString("full_name"),
            reader.GetString("role_code"),
            reader.GetString("role_name"),
            reader.GetString("department_code"),
            reader.GetString("department_name"),
            jwtTokenService.CreateToken(
                reader.GetInt32("id"),
                reader.GetString("login"),
                reader.GetString("full_name"),
                reader.GetString("role_code"),
                reader.GetString("department_code")));

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
