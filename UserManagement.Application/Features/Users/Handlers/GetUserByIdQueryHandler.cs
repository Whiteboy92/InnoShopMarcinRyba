using MediatR;
using Microsoft.AspNetCore.Identity;
using UserManagement.Application.DTOs;
using UserManagement.Application.Features.Users.Queries;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Features.Users.Handlers;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly UserManager<User> userManager;

    public GetUserByIdQueryHandler(UserManager<User> userManager)
    {
        this.userManager = userManager;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());

        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {request.UserId} not found.");
        }

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email ?? string.Empty,
            Role = user.Role,
            IsActive = user.IsActive,
        };
    }
}