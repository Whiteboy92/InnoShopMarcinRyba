using Microsoft.AspNetCore.Identity;
using UserManagement.Application.DTOs;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;

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
        
        var mappedRole = roles
            .Select(role => Enum.TryParse(role, out UserRole parsedRole) ? parsedRole : (UserRole?)null)
            .FirstOrDefault(role => role.HasValue) ?? UserRole.User;

        return new UserDto
        {
            Id = user.Id,
            Name = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Role = mappedRole,
            IsActive = user.LockoutEnd == null || user.LockoutEnd <= DateTime.UtcNow,
        };
    }
}