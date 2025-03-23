using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace UserManagement.Infrastructure.EmailService;

public class EmailService : IEmailService
{
    private readonly string smtpServer = "smtp.your-email-provider.com";  // Use your SMTP server
    private readonly int smtpPort = 587;  // SMTP Port, 587 is commonly used for TLS
    private readonly string smtpUser = "your-email@example.com";  // Your SMTP username
    private readonly string smtpPassword = "your-email-password";  // Your SMTP password
    private readonly ILogger<EmailService> logger;

    public EmailService(ILogger<EmailService> logger)
    {
        this.logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            logger.LogInformation("Sending email to {Email} with subject: {Subject}", to, subject);

            using var client = new SmtpClient(smtpServer, smtpPort);
                
            client.Credentials = new NetworkCredential(smtpUser, smtpPassword);
            client.EnableSsl = true;

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpUser),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);

            logger.LogInformation("Email sent successfully to {Email}", to);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while sending email to {Email}", to);
            throw new EmailSendException("An error occurred while sending the email.", ex);
        }
    }

    public async Task SendAccountVerificationEmailAsync(string userEmail, string token)
    {
        try
        {
            var verificationUrl = $"https://your-app.com/verify-email?token={token}";  // URL to verify the email
            var subject = "Please verify your email address";
            var body = $"Click the following link to verify your email address: <a href=\"{verificationUrl}\">Verify Email</a>";

            logger.LogInformation("Sending account verification email to {Email}", userEmail);

            await SendEmailAsync(userEmail, subject, body);

            logger.LogInformation("Account verification email sent to {Email}", userEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while sending account verification email to {Email}", userEmail);
            throw new EmailSendException("An error occurred while sending the account verification email.", ex);
        }
    }
}