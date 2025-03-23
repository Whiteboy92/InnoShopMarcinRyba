using MediatR;

namespace UserManagement.Application.Features.Users.Commands;

public class UpdateUserCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? NewRole { get; set; }
}