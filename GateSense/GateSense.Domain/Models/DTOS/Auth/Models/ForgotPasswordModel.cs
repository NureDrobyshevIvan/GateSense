namespace Domain.Models.DTOS.Auth.Models;

public class ForgotPasswordModel
{
    public string Email { get; set; }

    public string NewPassword { get; set; }

    public string PasswordToken { get; set; }
}