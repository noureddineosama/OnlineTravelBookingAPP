using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Security;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenGenerator(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public string GenerateToken(passenger user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKEY);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.email),
            new(JwtRegisteredClaimNames.Name, user.name),
            new(ClaimTypes.NameIdentifier, user.id.ToString()),
            new(ClaimTypes.Email, user.email),
            new(ClaimTypes.Name, user.name)
        };

        if (user.role is not null)
        {
            claims.Add(new Claim(ClaimTypes.Role, user.role.name));
        }
        else
        {
            // Default role fallback
            claims.Add(new Claim(ClaimTypes.Role, "Passenger"));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
