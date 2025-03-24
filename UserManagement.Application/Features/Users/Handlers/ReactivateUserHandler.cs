using MediatR;
using Microsoft.Extensions.Logging;
using ProductManagement.Persistence.Repositories;
using UserManagement.Application.Features.Users.Commands;
using UserManagement.Application.Interfaces;

namespace UserManagement.Application.Features.Users.Handlers;

public class ReactivateUserCommandHandler : IRequestHandler<ReactivateUserCommand, bool>
{
    private readonly IUserRepository userRepository;
    private readonly IProductRepository productRepository;
    private readonly ILogger<ReactivateUserCommandHandler> logger;

    public ReactivateUserCommandHandler(
        IUserRepository userRepository, 
        IProductRepository productRepository, 
        ILogger<ReactivateUserCommandHandler> logger)
    {
        this.userRepository = userRepository;
        this.productRepository = productRepository;
        this.logger = logger;
    }

    public async Task<bool> Handle(ReactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId)
                   ?? throw new KeyNotFoundException("User not found.");

        if (user.IsActive)
        {
            logger.LogWarning($"User {request.UserId} is already active.");
            return false;
        }

        user.IsActive = true;
        await userRepository.UpdateAsync(user);

        var userProducts = await productRepository.GetProductsByUserIdAsync(request.UserId);
        foreach (var product in userProducts)
        {
            product.IsDeleted = false;
            await productRepository.UpdateAsync(product);
        }

        await productRepository.SaveChangesAsync();

        logger.LogInformation($"User {request.UserId} reactivated. All products restored.");
        return true;
    }
}