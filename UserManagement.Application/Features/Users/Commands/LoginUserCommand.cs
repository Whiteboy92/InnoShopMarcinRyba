using MediatR;

namespace UserManagement.Application.Features.Users.Commands;

public class LoginUserCommand : IRequest<string>
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}