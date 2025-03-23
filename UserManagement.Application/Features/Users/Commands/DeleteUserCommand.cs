using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Features.Users.Commands;

public class DeleteUserCommand : IRequest<bool>
{
    public Guid Id { get; set; }

    public DeleteUserCommand(Guid id)
    {
        Id = id;
    }
}

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly UserManager<User> userManager;
    private readonly ILogger<DeleteUserCommandHandler> logger;

    public DeleteUserCommandHandler(UserManager<User> userManager, ILogger<DeleteUserCommandHandler> logger)
    {
        this.userManager = userManager;
        this.logger = logger;
    }

    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userManager.FindByIdAsync(request.Id.ToString());

            if (user == null)
            {
                logger.LogWarning("Attempted to delete a non-existent user with Id: {UserId}", request.Id);
                return false;
            }

            logger.LogInformation("Deleting user with Id: {UserId}", request.Id);

            var result = await userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                logger.LogInformation("User with Id: {UserId} successfully deleted.", request.Id);
                return true;
            }

            logger.LogError("Failed to delete user with Id: {UserId}. Errors: {Errors}", request.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting user with Id: {UserId}", request.Id);
            throw new Exception("An error occurred while deleting the user.", ex);
        }
    }
}