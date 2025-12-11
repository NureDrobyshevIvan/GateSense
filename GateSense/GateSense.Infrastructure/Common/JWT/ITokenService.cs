using Domain.Models.Auth;

namespace Infrastructure.Common.JWT;

public interface ITokenService
{
    public string GenerateAuthToken(ApplicationUser user);
    
    public RefreshTokenDTO GenerateRefreshToken();
}