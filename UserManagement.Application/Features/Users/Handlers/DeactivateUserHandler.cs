using MediatR;
using ProductManagement.Persistence.Repositories;
using Shared.Logging;
using UserManagement.Application.Features.Users.Commands;
using UserManagement.Application.Interfaces;

namespace UserManagement.Application.Features.Users.Handlers;

public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, bool>
{
    private readonly IUserRepository userRepository;
    private readonly IProductRepository productRepository;
    private readonly ILoggerService<DeactivateUserCommandHandler> loggerService;

    public DeactivateUserCommandHandler(
        IUserRepository userRepository, 
        IProductRepository productRepository, 
        ILoggerService<DeactivateUserCommandHandler> loggerService)
    {
        this.userRepository = userRepository;
        this.productRepository = productRepository;
        this.loggerService = loggerService;
    }

    public async Task<bool> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId);

        if (user == null)
        {
            loggerService.LogError($"User {request.UserId} not found.");
            throw new KeyNotFoundException("User not found.");
        }

        if (!user.IsActive)
        {
            loggerService.LogWarning($"User {request.UserId} is already deactivated.");
            return false;
        }

        user.IsActive = false;
        await userRepository.UpdateAsync(user);

        var userProducts = await productRepository.GetProductsByUserIdAsync(request.UserId);
        foreach (var product in userProducts)
        {
            product.IsDeleted = true;
            await productRepository.UpdateAsync(product);
        }

        await productRepository.SaveChangesAsync();

        loggerService.LogInformation($"User {request.UserId} deactivated and products marked as deleted.");
        return true;
    }
}