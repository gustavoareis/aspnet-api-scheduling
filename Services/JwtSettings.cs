using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace aspnet_api_scheduling.Services;

public sealed class JwtSettings
{
    private JwtSettings(string issuer, string audience, byte[] signingKey)
    {
        Issuer = issuer;
        Audience = audience;
        SigningKey = new SymmetricSecurityKey(signingKey);
    }

    public string Issuer { get; }
    public string Audience { get; }
    public SymmetricSecurityKey SigningKey { get; }

    public static JwtSettings Create(IConfiguration configuration, IHostEnvironment environment)
    {
        var key = configuration["Authentication:SigningKey"];
        if (string.IsNullOrWhiteSpace(key))
        {
            if (!environment.IsDevelopment())
            {
                throw new InvalidOperationException(
                    "Configure Authentication:SigningKey com um segredo de pelo menos 32 caracteres.");
            }

            key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        var keyBytes = Convert.FromBase64String(key);
        if (keyBytes.Length < 32)
        {
            throw new InvalidOperationException("Authentication:SigningKey deve ter pelo menos 32 bytes em Base64.");
        }

        return new JwtSettings(
            configuration["Authentication:Issuer"] ?? "barber-scheduling-api",
            configuration["Authentication:Audience"] ?? "barber-scheduling-clients",
            keyBytes);
    }
}
