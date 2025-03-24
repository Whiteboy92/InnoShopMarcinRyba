using Microsoft.AspNetCore.Identity;
using UserManagement.Application.DTOs;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Mapping;

public static class UserMappingExtensions
{
    public static async Task<UserDto> ToDto(this User user, UserManager<User> userManager)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user), "User cannot be null.");
        }

        var roles = await userManager.GetRolesAsync(user);
        
        return new UserDto
        {
            Id = user.Id,
            Name = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Role = roles.FirstOrDefault() ?? "User",
            IsActive = user.LockoutEnd == null || user.LockoutEnd <= DateTime.UtcNow,
        };
    }
}