namespace UserManagement.Application.Features.Users.Commands;

public class PasswordRecoveryRequest
{
    public required string Email { get; set; }
}