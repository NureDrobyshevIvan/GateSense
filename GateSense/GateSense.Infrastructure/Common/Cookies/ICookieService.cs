namespace Infrastructure.Common.Cookies;

public interface ICookieService
{
    public void SetAuthCookies(string accessToken, string refreshToken);

    public void ClearAuthCookies();
    
    public string? GetRefreshToken();
}