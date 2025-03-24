using Microsoft.AspNetCore.Identity;
using ProductManagement.Persistence.Repositories;
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
    private readonly IProductRepository productRepository;
    private readonly UserManager<User> userManager;
    private readonly ILoggerService<UserService> logger;

    public UserService(
        IUserRepository userRepository,
        IProductRepository productRepository,
        UserManager<User> userManager,
        ILoggerService<UserService> logger)
    {
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        this.productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserDto> GetUserByIdAsync(Guid userId)
    {
        logger.LogInformation($"Fetching user by ID: {userId}");

        var user = await userRepository.GetByIdAsync(userId) ?? throw new KeyNotFoundException("User not found.");
        return await user.ToDto(userManager);
    }

    public async Task<UserDto[]> GetAllUsersAsync()
    {
        logger.LogInformation("Fetching all users.");
        var users = await userRepository.GetAllAsync();
        return await Task.WhenAll(users.Select(user => user.ToDto(userManager)));
    }

    public async Task<bool> CreateUserAsync(CreateUserCommand command)
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
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        await userManager.AddToRoleAsync(user, "User");
        logger.LogInformation($"User {command.Email} created successfully.");
        return true;
    }

    public async Task<bool> UpdateUserAsync(UpdateUserCommand command)
    {
        logger.LogInformation($"Updating user {command.Id}");

        var user = await userRepository.GetByIdAsync(command.Id) ?? throw new KeyNotFoundException("User not found.");
        user.Name = command.Name;
        user.Email = command.Email;
        user.UserName = command.Email;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        logger.LogInformation($"User {command.Id} updated successfully.");
        return true;
    }

    public async Task<bool> DeactivateUserAsync(Guid userId)
    {
        logger.LogInformation($"Deactivating user {userId}.");

        var user = await userRepository.GetByIdAsync(userId) 
                   ?? throw new KeyNotFoundException("User not found.");

        if (!user.IsActive)
        {
            logger.LogWarning($"User {userId} is already deactivated.");
            return false;
        }

        user.IsActive = false;
        await userRepository.UpdateAsync(user);

        var userProducts = await productRepository.GetProductsByUserIdAsync(userId);

        if (userProducts.Any())
        {
            foreach (var product in userProducts)
            {
                product.IsDeleted = true;
                await productRepository.UpdateAsync(product);
            }

            await productRepository.SaveChangesAsync();
            logger.LogInformation($"User {userId} deactivated. All products hidden.");
        }
        else
        {
            logger.LogInformation($"User {userId} deactivated. No products to hide.");
        }

        return true;
    }

    public async Task<bool> ReactivateUserAsync(Guid userId)
    {
        logger.LogInformation($"Reactivating user {userId}.");

        var user = await userRepository.GetByIdAsync(userId)
                   ?? throw new KeyNotFoundException("User not found.");

        if (user.IsActive)
        {
            logger.LogWarning($"User {userId} is already active.");
            return false;
        }

        user.IsActive = true;
        await userRepository.UpdateAsync(user);

        var userProducts = await productRepository.GetProductsByUserIdAsync(userId);

        if (userProducts.Any())
        {
            foreach (var product in userProducts)
            {
                product.IsDeleted = false;
                await productRepository.UpdateAsync(product);
            }

            await productRepository.SaveChangesAsync();
            logger.LogInformation($"User {userId} reactivated. All products restored.");
        }
        else
        {
            logger.LogInformation($"User {userId} reactivated. No products to restore.");
        }

        return true;
    }

    public async Task<bool> AssignRoleToUserAsync(Guid userId, string role)
    {
        logger.LogInformation($"Assigning role '{role}' to user {userId}");

        var user = await userRepository.GetByIdAsync(userId) ?? throw new KeyNotFoundException("User not found.");
        var result = await userManager.AddToRoleAsync(user, role);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        logger.LogInformation($"Role '{role}' assigned to user {userId} successfully.");
        return true;
    }

    public async Task<bool> ChangeUserPasswordAsync(Guid userId, string newPassword)
    {
        logger.LogInformation($"Changing password for user {userId}");

        var user = await userRepository.GetByIdAsync(userId) ?? throw new KeyNotFoundException("User not found.");
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, newPassword);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        logger.LogInformation($"Password changed successfully for user {userId}");
        return true;
    }

    public async Task<UserDto> GetUserByEmailAsync(string userEmail)
    {
        logger.LogInformation($"Fetching user by email: {userEmail}");

        try
        {
            var user = await userRepository.GetByEmailAsync(userEmail) 
                       ?? throw new KeyNotFoundException($"User with email {userEmail} not found.");

            var userDto = await user.ToDto(userManager);
            logger.LogInformation($"User found: {userEmail}");
        
            return userDto;
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogError($"User with email {userEmail} not found.");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError($"An error occurred while fetching the user with email {userEmail}");
            throw new ApplicationException("An error occurred while retrieving the user information.", ex);
        }
    }
}
