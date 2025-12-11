namespace Infrastructure.Common.JWT;

public class RefreshTokenDTO
{
    public string bytes { get; set; }
    
    public int ExpirationTimeInDays { get; set; }
}