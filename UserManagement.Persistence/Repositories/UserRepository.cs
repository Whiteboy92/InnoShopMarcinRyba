using Microsoft.AspNetCore.Identity;
using UserManagement.Application.Interfaces;
using UserManagement.Domain.Entities;
using Shared.Logging;
using UserManagement.Application.DTOs;

namespace UserManagement.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<User> userManager;
    private readonly ILoggerService<UserRepository> logger;

    public UserRepository(UserManager<User> userManager, ILoggerService<UserRepository> logger)
    {
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<User> GetByIdAsync(Guid userId)
    {
        try
        {
            logger.LogInformation($"Fetching user by ID: {userId}");

            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                logger.LogWarning($"User with ID {userId} not found.");
                throw new InvalidOperationException("User not found.");
            }

            return user;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error fetching user by ID: {userId}. Exception: {ex.Message}");
            throw new ApplicationException("An error occurred while fetching the user.", ex);
        }
    }

    public Task<List<User>> GetAllAsync()
    {
        try
        {
            logger.LogInformation("Fetching all users.");

            return Task.FromResult(userManager.Users.ToList());
        }
        catch (Exception ex)
        {
            logger.LogError($"Error fetching all users. Exception: {ex.Message}");
            throw new ApplicationException("An error occurred while fetching the users.", ex);
        }
    }

    public async Task<bool> CreateAsync(User user)
    {
        try
        {
            logger.LogInformation($"Creating user: {user.Email}");

            var result = await userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                logger.LogWarning($"Failed to create user {user.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                return false;
            }

            logger.LogInformation($"User {user.Email} created successfully.");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error creating user {user.Email}. Exception: {ex.Message}");
            throw new ApplicationException("An error occurred while creating the user.", ex);
        }
    }

    public async Task<bool> UpdateAsync(User user)
    {
        try
        {
            logger.LogInformation($"Updating user {user.Id}");

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                logger.LogWarning($"Failed to update user {user.Id}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                return false;
            }

            logger.LogInformation($"User {user.Id} updated successfully.");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error updating user {user.Id}. Exception: {ex.Message}");
            throw new ApplicationException("An error occurred while updating the user.", ex);
        }
    }

    public async Task<bool> DeleteAsync(Guid userId)
    {
        try
        {
            logger.LogInformation($"Deleting user {userId}");

            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                logger.LogWarning($"User with ID {userId} not found.");
                return false;
            }

            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                logger.LogWarning($"Failed to delete user {userId}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                return false;
            }

            logger.LogInformation($"User {userId} deleted successfully.");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error deleting user {userId}. Exception: {ex.Message}");
            throw new ApplicationException("An error occurred while deleting the user.", ex);
        }
    }

    public async Task<User> GetByEmailAsync(string userEmail)
    {
        try
        {
            logger.LogInformation($"Fetching user by email: {userEmail}");

            var user = await userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                logger.LogWarning($"User with email {userEmail} not found.");
                throw new InvalidOperationException("User not found by email.");
            }

            return user;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error fetching user by email: {userEmail}. Exception: {ex.Message}");
            throw new ApplicationException("An error occurred while fetching the user by email.", ex);
        }
    }
}