using Moq;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Features.Products.Commands;
using ProductManagement.Application.Features.Products.Handlers;
using ProductManagement.Application.Interfaces;
using Shared.Logging;

namespace ProductManagement.Tests.Commands
{
    public class DeleteProductHandlerTests
    {
        private readonly Mock<IProductService> mockProductService;
        private readonly Mock<ILoggerService<DeleteProductHandler>> mockLogger;
        private readonly DeleteProductHandler handler;

        public DeleteProductHandlerTests()
        {
            mockProductService = new Mock<IProductService>();
            mockLogger = new Mock<ILoggerService<DeleteProductHandler>>();
            handler = new DeleteProductHandler(mockProductService.Object, mockLogger.Object);
        }

        [Fact]
        public async Task Handle_ShouldDeleteProductSuccessfully()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new DeleteProductCommand(productId);

            // Simulate product exists and deletion succeeds
            mockProductService.Setup(productService => productService.GetByIdAsync(productId))
                .ReturnsAsync(new ProductDto
                {
                    Id = productId,
                    Name = "Test Product",
                    Description = null,
                    Price = 1234,
                });
            mockProductService.Setup(productService => productService.DeleteAsync(productId))
                .ReturnsAsync(true);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            mockLogger.Verify(x => x.LogInformation(It.Is<string>(msg 
                => msg.Contains($"Attempting to delete product with ID: {productId}"))), Times.Once);
            
            mockLogger.Verify(x => x.LogInformation(It.Is<string>(msg 
                => msg.Contains($"Product with ID: {productId} deleted successfully."))), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenProductNotFound()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new DeleteProductCommand(productId);

            // Simulate product not found
            mockProductService.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync((ProductDto)null!);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result);
            mockLogger.Verify(loggerService => loggerService.LogWarning(It.Is<string>(msg 
                => msg.Contains($"Product with ID: {productId} not found."))), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new DeleteProductCommand(productId);

            // Simulate an exception during the deletion process
            mockProductService.Setup(x => x.GetByIdAsync(productId))
                .ThrowsAsync(new Exception("Test Exception"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApplicationException>(() => handler.Handle(command, CancellationToken.None));

            // Verify that LogError was called once with any string and exception
            mockLogger.Verify(loggerService => loggerService.LogError(It.IsAny<string>(), It.IsAny<Exception?>()), Times.Once);

            // Optionally, check if the exception message contains the expected error string
            Assert.Contains("Error occurred while deleting the product", exception.Message);
        }
    }
}
