using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PlantProduction.Api.Security;

public sealed class JwtTokenService(IConfiguration configuration)
{
    public string CreateToken(int userId, string login, string fullName, string roleCode, string departmentCode)
    {
        var issuer = configuration["Jwt:Issuer"] ?? "PlantProduction.Api";
        var audience = configuration["Jwt:Audience"] ?? "PlantProduction.Client";
        var key = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT key is not configured.");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, login),
            new("full_name", fullName),
            new(ClaimTypes.Role, roleCode),
            new("department", departmentCode)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
