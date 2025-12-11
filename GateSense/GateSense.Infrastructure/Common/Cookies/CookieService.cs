using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Common.Cookies;

public class CookieService : ICookieService
{
    private readonly CookieOptions _accessTokenOptions;
    private readonly CookieOptions _refreshTokenOptions;
    private readonly HttpResponse _httpResponse;
    private readonly HttpRequest _httpRequest;
    private readonly CookieConfig _cookieConfig;

    public CookieService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _cookieConfig = configuration.GetSection("CookieOptions").Get<CookieConfig>() ??  throw new InvalidOperationException();
        _httpRequest = httpContextAccessor.HttpContext?.Request ??  throw new InvalidOperationException();
        _httpResponse = httpContextAccessor.HttpContext?.Response ??  throw new InvalidOperationException();
        _accessTokenOptions = new CookieOptions
        {
            HttpOnly = true,
            //Secure = true, //commented out for DEV
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddMinutes(_cookieConfig.AccessTokenLifetimeInMinutes) 
        };

        _refreshTokenOptions = new CookieOptions
        {
            HttpOnly = true,
            //Secure = true, //commented out for DEV
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(_cookieConfig.RefreshTokenLifeTimeInDays) 
        };
    }

    public void SetAuthCookies(string accessToken, string refreshToken)
    {
        _httpResponse.Cookies.Append(AppConstants.AccessTokenCookie, accessToken, _accessTokenOptions);
        _httpResponse.Cookies.Append(AppConstants.RefreshTokenCookie, refreshToken, _refreshTokenOptions);
    }

    public void ClearAuthCookies()
    {
        var expired = new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddDays(-1),
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        };

        _httpResponse.Cookies.Append(AppConstants.AccessTokenCookie, "", expired);
        _httpResponse.Cookies.Append(AppConstants.RefreshTokenCookie, "", expired);
    }

    public string? GetRefreshToken()
    {
        _httpRequest.Cookies.TryGetValue(AppConstants.RefreshTokenCookie, out var token);
        return token;
    }
}