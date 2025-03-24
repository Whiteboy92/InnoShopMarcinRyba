using Moq;
using ProductManagement.Domain.Entities;
using ProductManagement.Persistence.Repositories;
using Shared.Logging;
using UserManagement.Application.Features.Users.Commands;
using UserManagement.Application.Features.Users.Handlers;
using UserManagement.Application.Interfaces;
using UserManagement.Domain.Entities;

namespace UserManagement.Tests.Commands
{
    public class DeactivateUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> mockUserRepository;
        private readonly Mock<IProductRepository> mockProductRepository;
        private readonly Mock<ILoggerService<DeactivateUserCommandHandler>> mockLoggerService;
        private readonly DeactivateUserCommandHandler handler;

        public DeactivateUserCommandHandlerTests()
        {
            mockUserRepository = new Mock<IUserRepository>();
            mockProductRepository = new Mock<IProductRepository>();
            mockLoggerService = new Mock<ILoggerService<DeactivateUserCommandHandler>>();
            handler = new DeactivateUserCommandHandler(
                mockUserRepository.Object,
                mockProductRepository.Object,
                mockLoggerService.Object);
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var command = new DeactivateUserCommand(Guid.NewGuid());
            mockUserRepository.Setup(repo => repo.GetByIdAsync(command.UserId))
                .ReturnsAsync((User)null!);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));

            // Assert that the exception message matches
            Assert.Equal("User not found.", exception.Message);

            // Verify that the error log was recorded when user was not found
            mockLoggerService.Verify(
                logger => logger.LogError(It.Is<string>(msg => msg.Contains("User") && msg.Contains("not found")),
                    It.IsAny<Exception>()),
                Times.Once
            );
        }
        
        [Fact]
        public async Task Handle_DeactivatesUserAndProducts_ReturnsTrue()
        {
            // Arrange
            var command = new DeactivateUserCommand(Guid.NewGuid());
            var user = new User
            {
                Id = command.UserId,
                IsActive = true,
                Name = "Test User",
            };
            var products = new List<Product>
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    CreatorUserId = command.UserId,
                    IsDeleted = false,
                    Name = "Test Product",
                    Description = null,
                    Price = 15,
                },
            };

            mockUserRepository.Setup(repo => repo.GetByIdAsync(command.UserId))
                .ReturnsAsync(user);
            mockProductRepository.Setup(repo => repo.GetProductsByUserIdAsync(command.UserId))
                .ReturnsAsync(products);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.All(products, product => Assert.True(product.IsDeleted));

            // Verify that the correct log message was logged
            mockLoggerService.Verify(logger =>
                logger.LogInformation(It.Is<string>(msg => msg.Contains($"User {user.Id} deactivated and products marked as deleted"))),
                Times.Once);
        }
    }
}
