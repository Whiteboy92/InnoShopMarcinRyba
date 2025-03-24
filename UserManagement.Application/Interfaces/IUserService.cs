using UserManagement.Application.DTOs;
using UserManagement.Application.Features.Users.Commands;

namespace UserManagement.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> GetUserByIdAsync(Guid userId);
    Task<UserDto[]> GetAllUsersAsync();
    Task<bool> CreateUserAsync(CreateUserCommand command);
    Task<bool> UpdateUserAsync(UpdateUserCommand command);
    Task<bool> DeactivateUserAsync(Guid userId);
    Task<bool> ReactivateUserAsync(Guid userId);
    Task<bool> AssignRoleToUserAsync(Guid userId, string role);
    Task<bool> ChangeUserPasswordAsync(Guid userId, string newPassword);
}