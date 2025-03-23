using MediatR;
using Microsoft.AspNetCore.Identity;
using UserManagement.Application.DTOs;
using UserManagement.Application.Features.Users.Queries;
using UserManagement.Application.Mapping;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Features.Users.Handlers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
{
    private readonly UserManager<User> userManager;

    public GetAllUsersQueryHandler(UserManager<User> userManager)
    {
        this.userManager = userManager;
    }

    public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = userManager.Users.ToList();
            
        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            var userDto = await user.ToDto(userManager);
            userDtos.Add(userDto);
        }

        return userDtos;
    }
}