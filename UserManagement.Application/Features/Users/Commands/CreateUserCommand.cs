using MediatR;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Features.Users.Commands;

public class CreateUserCommand : IRequest<User>
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Role { get; set; }
}