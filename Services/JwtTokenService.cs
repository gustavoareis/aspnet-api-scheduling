using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using aspnet_api_scheduling.Models;
using Microsoft.IdentityModel.Tokens;

namespace aspnet_api_scheduling.Services;

public sealed class JwtTokenService(JwtSettings settings)
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(20);

    public (string Token, DateTimeOffset ExpiresAt) Create(Usuario usuario, int barbeariaId, string papel)
    {
        var expiresAt = DateTimeOffset.UtcNow.Add(TokenLifetime);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email ?? string.Empty),
            new Claim("barbearia_id", barbeariaId.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new Claim(ClaimTypes.Role, papel)
        };
        var credentials = new SigningCredentials(settings.SigningKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            settings.Issuer,
            settings.Audience,
            claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);
        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
