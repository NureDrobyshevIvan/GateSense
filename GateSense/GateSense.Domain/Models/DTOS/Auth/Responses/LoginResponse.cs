namespace Domain.Models.DTOS.Auth.Responses;

public class LoginResponse
{
    public string UserName { get; set; }

    public string Email { get; set; }

    public required string Role { get; set; }

    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }
}