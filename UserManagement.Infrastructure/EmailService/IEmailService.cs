namespace UserManagement.Infrastructure.EmailService;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendAccountVerificationEmailAsync(string userEmail, string token);
    Task SendPasswordRecoveryEmailAsync(string userEmail, string token);
}
