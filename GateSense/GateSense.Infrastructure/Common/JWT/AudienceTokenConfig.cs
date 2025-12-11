namespace Infrastructure.Common.JWT;

public class AudienceTokenConfig
{
    public string JwtAudience { get; set; }

    public string JwtIssuer { get; set; }

    public string JwtKey { get; set; }

    public int JwtExpirationInMinutes { get; set; }
    
    public int RefreshTokenExpirationInDays { get; set; }
}