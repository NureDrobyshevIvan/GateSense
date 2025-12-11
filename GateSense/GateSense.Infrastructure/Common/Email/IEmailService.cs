namespace Infrastructure.Common.Email;

public interface IEmailService
{
    public Task SendConfirmationLinkAsync(string email, string confirmationToken);

    public Task SendSuccessfulEmailAsync(string email, string message, string subject);

    public Task SendErrorEmailAsync(string email, string message, string subject);

    public Task SendForgotPasswordLinkAsync(string email, string passwordToken);
}