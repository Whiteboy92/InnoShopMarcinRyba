using MediatR;

namespace UserManagement.Application.Features.Users.Commands;

public class ReactivateUserCommand : IRequest<bool>
{
    public Guid UserId { get; }

    public ReactivateUserCommand(Guid userId)
    {
        UserId = userId;
    }
}