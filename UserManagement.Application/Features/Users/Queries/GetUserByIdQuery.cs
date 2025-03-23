using MediatR;
using UserManagement.Application.DTOs;

namespace UserManagement.Application.Features.Users.Queries;

public class GetUserByIdQuery : IRequest<UserDto>
{
    public Guid UserId { get; }

    public GetUserByIdQuery(Guid userId)
    {
        UserId = userId;
    }
}