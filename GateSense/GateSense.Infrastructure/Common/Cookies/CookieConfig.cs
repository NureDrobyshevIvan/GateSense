namespace Infrastructure.Common.Cookies;

public class CookieConfig
{
    public int AccessTokenLifetimeInMinutes { get; set; }
    
    public int RefreshTokenLifeTimeInDays { get; set; }
}