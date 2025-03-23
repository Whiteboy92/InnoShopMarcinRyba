using MediatR;
using UserManagement.Application.DTOs;

namespace UserManagement.Application.Features.Users.Queries;

public class GetAllUsersQuery : IRequest<List<UserDto>>
{
}