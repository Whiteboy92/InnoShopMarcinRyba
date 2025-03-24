namespace UserManagement.Application.Interfaces;

public interface IPasswordRecoveryService
{
    public Task SendPasswordRecoveryEmailAsync(string userEmail);
}