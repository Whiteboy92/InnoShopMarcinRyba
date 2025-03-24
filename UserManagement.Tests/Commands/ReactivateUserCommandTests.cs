using Microsoft.Extensions.Logging;
using Moq;
using ProductManagement.Domain.Entities;
using ProductManagement.Persistence.Repositories;
using UserManagement.Application.Features.Users.Commands;
using UserManagement.Application.Features.Users.Handlers;
using UserManagement.Application.Interfaces;
using UserManagement.Domain.Entities;

namespace UserManagement.Tests.Commands;

public class ReactivateUserCommandTests
{
    private readonly Mock<IUserRepository> userRepositoryMock;
    private readonly Mock<IProductRepository> productRepositoryMock;
    private readonly ReactivateUserCommandHandler handler;

    public ReactivateUserCommandTests()
    {
        // Arrange mocks
        userRepositoryMock = new Mock<IUserRepository>();
        productRepositoryMock = new Mock<IProductRepository>();
        Mock<ILogger<ReactivateUserCommandHandler>> loggerMock1 = new Mock<ILogger<ReactivateUserCommandHandler>>();

        // Initialize the handler with mocked dependencies
        handler = new ReactivateUserCommandHandler(
            userRepositoryMock.Object,
            productRepositoryMock.Object,
            loggerMock1.Object
        );
    }

    [Fact]
    public async Task Handle_WhenUserIsInactive_ShouldReactivateUserAndRestoreProducts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            IsActive = false,
            Name = "Test User",
            Role = "User",
        };
        var products = new List<Product>
        {
            new Product
            {
                Id = userId,
                IsDeleted = true,
                Name = "Test Product1",
                Description = null,
                Price = 115,
            },
            new Product
            {
                Id = userId,
                IsDeleted = true,
                Name = "Test Product2",
                Description = null,
                Price = 50,
            },
        };

        userRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(user);
        productRepositoryMock.Setup(repo => repo.GetProductsByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(products);
        productRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

        var command = new ReactivateUserCommand(userId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        userRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<User>(u => u.Id == userId && u.IsActive == true)), Times.Once);
        productRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<Product>(p => p.Id == userId && p.IsDeleted == false)), Times.Exactly(2));
        productRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsAlreadyActive_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            IsActive = true,
            Name = "Test User",
            Role = "User",
        };

        userRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(user);

        var command = new ReactivateUserCommand(userId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
        productRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        userRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User)null!);

        var command = new ReactivateUserCommand(userId);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }
}