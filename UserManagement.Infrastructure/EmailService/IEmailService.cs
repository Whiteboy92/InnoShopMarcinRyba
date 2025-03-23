namespace UserManagement.Infrastructure.EmailService;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendAccountVerificationEmailAsync(string userEmail, string token);
}