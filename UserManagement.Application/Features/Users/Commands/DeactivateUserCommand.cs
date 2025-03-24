using MediatR;

namespace UserManagement.Application.Features.Users.Commands;

public class DeactivateUserCommand : IRequest<bool>
{
    public Guid UserId { get; set; }

    public DeactivateUserCommand(Guid userId)
    {
        UserId = userId;
    }
}