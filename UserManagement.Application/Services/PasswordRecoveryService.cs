using UserManagement.Application.Interfaces;
using UserManagement.Infrastructure.EmailService;

namespace UserManagement.Application.Services;

public abstract class PasswordRecoveryService : IPasswordRecoveryService
{
    private readonly IEmailService emailService;
    private readonly IUserService userService;

    protected PasswordRecoveryService(IEmailService emailService, IUserService userService)
    {
        this.emailService = emailService;
        this.userService = userService;
    }

    public async Task SendPasswordRecoveryEmailAsync(string userEmail)
    {
        var user = await userService.GetUserByEmailAsync(userEmail);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }
        
        var token = Guid.NewGuid().ToString();
        
        await emailService.SendPasswordRecoveryEmailAsync(userEmail, token);
    }
}