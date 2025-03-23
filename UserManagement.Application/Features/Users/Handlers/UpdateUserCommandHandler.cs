using System.ComponentModel.DataAnnotations;
using Hellang.Middleware.ProblemDetails;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Features.Users.Commands;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Features.Users.Handlers;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, bool>
{
    private readonly UserManager<User> userManager;
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly ILogger<UpdateUserCommandHandler> logger;

    public UpdateUserCommandHandler(
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<UpdateUserCommandHandler> logger)
    {
        this.userManager = userManager;
        this.roleManager = roleManager;
        this.logger = logger;
    }

    public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate email format first
            if (!new EmailAddressAttribute().IsValid(request.Email))
            {
                throw new ProblemDetailsException(new ProblemDetails
                {
                    Status = 400,
                    Title = "Validation Error",
                    Detail = "Invalid email format.",
                });
            }

            var user = await userManager.FindByIdAsync(request.Id.ToString());
            if (user == null)
            {
                throw new ProblemDetailsException(new ProblemDetails
                {
                    Status = 404,
                    Title = "Not Found",
                    Detail = $"User with Id {request.Id} not found.",
                });
            }

            // Update user properties
            user.Name = request.Name;
            user.Email = request.Email;
            user.UserName = request.Email;
            await userManager.UpdateNormalizedEmailAsync(user);

            // Process role update if requested
            if (!string.IsNullOrWhiteSpace(request.NewRole))
            {
                if (!await roleManager.RoleExistsAsync(request.NewRole))
                {
                    throw new ProblemDetailsException(new ProblemDetails
                    {
                        Status = 400,
                        Title = "Validation Error",
                        Detail = $"Role '{request.NewRole}' does not exist.",
                    });
                }

                var currentRoles = await userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                {
                    var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removeResult.Succeeded)
                    {
                        throw new ProblemDetailsException(new ProblemDetails
                        {
                            Status = 400,
                            Title = "Identity Error",
                            Detail = string.Join(", ", removeResult.Errors.Select(e => e.Description)),
                        });
                    }
                }

                var addResult = await userManager.AddToRoleAsync(user, request.NewRole);
                if (!addResult.Succeeded)
                {
                    throw new ProblemDetailsException(new ProblemDetails
                    {
                        Status = 400,
                        Title = "Identity Error",
                        Detail = string.Join(", ", addResult.Errors.Select(e => e.Description)),
                    });
                }
            }

            // Save user changes
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                throw new ProblemDetailsException(new ProblemDetails
                {
                    Status = 400,
                    Title = "Identity Error",
                    Detail = string.Join(", ", updateResult.Errors.Select(e => e.Description)),
                });
            }

            logger.LogInformation("User {UserId} updated successfully.", request.Id);
            return true;
        }
        catch (ProblemDetailsException)
        {
            throw; // Re-throw already handled exceptions
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user {UserId}", request.Id);
            throw new ProblemDetailsException(new ProblemDetails
            {
                Status = 500,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred.",
            });
        }
    }
}