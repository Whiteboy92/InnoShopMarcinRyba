using Microsoft.AspNetCore.Identity;
using UserManagement.Application.Interfaces;
using UserManagement.Application.DTOs;
using UserManagement.Application.Features.Users.Commands;
using UserManagement.Application.Mapping;
using UserManagement.Domain.Entities;
using Shared.Logging;

namespace UserManagement.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository userRepository;
    private readonly UserManager<User> userManager;
    private readonly ILoggerService<UserService> logger;

    public UserService(IUserRepository userRepository, UserManager<User> userManager, ILoggerService<UserService> logger)
    {
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserDto> GetUserByIdAsync(Guid userId)
    {
        try
        {
            logger.LogInformation($"Fetching user by ID: {userId}");

            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                logger.LogWarning($"User with ID {userId} not found.");
                throw new KeyNotFoundException("User not found.");
            }

            return await user.ToDto(userManager);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning($"User with ID {userId} not found. Exception: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error fetching user by ID: {userId}. Exception: {ex.Message}");
            throw new ApplicationException("An error occurred while retrieving the user.", ex);
        }
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        try
        {
            logger.LogInformation("Fetching all users.");

            var users = await userRepository.GetAllAsync();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                userDtos.Add(await user.ToDto(userManager));
            }

            return userDtos;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error fetching all users. Exception: {ex.Message}");
            throw new ApplicationException("An error occurred while retrieving the users.", ex);
        }
    }

    public async Task<bool> CreateUserAsync(CreateUserCommand command)
    {
        try
        {
            logger.LogInformation($"Creating user: {command.Email}");

            var user = new User
            {
                UserName = command.Email,
                Email = command.Email,
                Name = command.Name,
            };

            var result = await userManager.CreateAsync(user, command.Password);
            if (!result.Succeeded)
            {
                logger.LogWarning($"Failed to create user {command.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                throw new InvalidOperationException($"Failed to create user {command.Email}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            var roleResult = await userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                logger.LogWarning($"Failed to assign role to user {command.Email}: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                throw new InvalidOperationException($"Failed to assign role to user {command.Email}. Errors: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }

            logger.LogInformation($"User {command.Email} created successfully.");
            return true;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError($"Error creating user {command.Email}. Exception: {ex.Message}");
            throw new ApplicationException("An error occurred while creating the user.", ex);
        }
        catch (Exception ex)
        {
            logger.LogError($"Error creating user {command.Email}. Exception: {ex.Message}");
            throw new ApplicationException("An error occurred while creating the user.", ex);
        }
    }

    public async Task<bool> UpdateUserAsync(UpdateUserCommand command)
    {
        try
        {
            logger.LogInformation($"Updating user {command.Id}");

            var user = await userRepository.GetByIdAsync(command.Id);
            if (user == null)
            {
                logger.LogWarning($"User with ID {command.Id} not found.");
                throw new KeyNotFoundException("User not found.");
            }

            user.Name = command.Email;
            user.Email = command.Email;
            user.UserName = command.Name;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                logger.LogWarning($"Failed to update user {command.Id}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                throw new InvalidOperationException($"Failed to update user {command.Id}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            logger.LogInformation($"User {command.Id} updated successfully.");
            return true;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError($"Error updating user {command.Id}. Exception: {ex.Message}");
            throw new ApplicationException("An error occurred while updating the user.", ex);
        }
        catch (Exception ex)
        {
            logger.LogError($"Error updating user {command.Id}. Exception: {ex.Message}");
            throw new ApplicationException("An error occurred while updating the user.", ex);
        }
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        try
        {
            logger.LogInformation($"Deleting user {userId}");

            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                logger.LogWarning($"User with ID {userId} not found.");
                throw new KeyNotFoundException("User not found.");
            }

            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                logger.LogWarning($"Failed to delete user {userId}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                throw new InvalidOperationException($"Failed to delete user {userId}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            logger.LogInformation($"User {userId} deleted successfully.");
            return true;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError($"Error deleting user {userId}. Exception: {ex.Message}");
            throw new ApplicationException("An error occurred while deleting the user.", ex);
        }
        catch (Exception ex)
        {
            logger.LogError($"Error deleting user {userId}. Exception: {ex.Message}");
            throw new ApplicationException("An error occurred while deleting the user.", ex);
        }
    }

    public async Task<bool> AssignRoleToUserAsync(Guid userId, string role)
    {
        try
        {
            logger.LogInformation($"Assigning role '{role}' to user {userId}");

            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                logger.LogWarning($"User with ID {userId} not found.");
                throw new KeyNotFoundException("User not found.");
            }

            var result = await userManager.AddToRoleAsync(user, role);
            if (!result.Succeeded)
            {
                logger.LogWarning($"Failed to assign role '{role}' to user {userId}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                throw new InvalidOperationException($"Failed to assign role '{role}' to user {userId}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            logger.LogInformation($"Role '{role}' assigned to user {userId} successfully.");
            return true;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError($"Error assigning role to user {userId}. Exception: {ex.Message}");
            throw new ApplicationException("An error occurred while assigning role to the user.", ex);
        }
        catch (Exception ex)
        {
            logger.LogError($"Error assigning role '{role}' to user {userId}. Exception: {ex.Message}");
            throw new ApplicationException("An error occurred while assigning role to the user.", ex);
        }
    }

    public async Task<bool> ChangeUserPasswordAsync(Guid userId, string newPassword)
    {
        try
        {
            logger.LogInformation($"Changing password for user {userId}");

            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                logger.LogWarning($"User with ID {userId} not found.");
                throw new KeyNotFoundException("User not found.");
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
            {
                logger.LogWarning($"Failed to change password for user {userId}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                throw new InvalidOperationException($"Failed to change password for user {userId}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            logger.LogInformation($"Password changed successfully for user {userId}");
            return true;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError($"Error changing password for user {userId}. Exception: {ex.Message}");
            throw new ApplicationException("An error occurred while changing the password.", ex);
        }
        catch (Exception ex)
        {
            logger.LogError($"Error changing password for user {userId}. Exception: {ex.Message}");
            throw new ApplicationException("An error occurred while changing the password.", ex);
        }
    }
}