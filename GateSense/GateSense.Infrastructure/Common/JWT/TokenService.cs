using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Domain.Models.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Common.JWT;

public class TokenService : ITokenService
{
    private readonly AudienceTokenConfig _audienceTokenConfig;

    public TokenService(IConfiguration config)
    {
        _audienceTokenConfig = config.GetSection("AudienceTokenConfig").Get<AudienceTokenConfig>();
    }

    public string GenerateAuthToken(ApplicationUser user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_audienceTokenConfig.JwtKey));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
            ]),
            Expires = DateTime.UtcNow.AddMinutes(_audienceTokenConfig.JwtExpirationInMinutes),
            SigningCredentials = credentials,
            Issuer = _audienceTokenConfig.JwtIssuer,
            Audience = _audienceTokenConfig.JwtAudience,
        };

        var tokenHandler = new JsonWebTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return token;
    }

    public RefreshTokenDTO GenerateRefreshToken()
    {
        var bytes = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        var dto = new RefreshTokenDTO
        {
            bytes = Base64UrlEncoder.Encode(bytes),
            ExpirationTimeInDays = _audienceTokenConfig.RefreshTokenExpirationInDays
        };

        return dto;
    }
}