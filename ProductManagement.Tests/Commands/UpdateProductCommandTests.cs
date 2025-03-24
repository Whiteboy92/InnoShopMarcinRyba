using Moq;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Features.Products.Commands;
using ProductManagement.Application.Features.Products.Handlers;
using ProductManagement.Application.Interfaces;
using Shared.Logging;

namespace ProductManagement.Tests.Commands;

public class UpdateProductHandlerTests
{
    private readonly Mock<IProductService> mockProductService;
    private readonly Mock<ILoggerService<UpdateProductHandler>> mockLogger;
    private readonly UpdateProductHandler handler;

    public UpdateProductHandlerTests()
    {
        mockProductService = new Mock<IProductService>();
        mockLogger = new Mock<ILoggerService<UpdateProductHandler>>();
        handler = new UpdateProductHandler(mockProductService.Object, mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateProductSuccessfully()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 150,
            IsAvailable = false
        };

        var existingProduct = new ProductDto
        {
            Id = command.Id,
            Name = "Old Product",
            Description = "Old Description",
            Price = 100,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };

        var updatedProduct = new ProductDto
        {
            Id = command.Id,
            Name = command.Name,
            Description = command.Description,
            Price = command.Price,
            IsAvailable = command.IsAvailable,
            CreatedAt = existingProduct.CreatedAt
        };

        mockProductService.Setup(x => x.GetByIdAsync(command.Id))
            .ReturnsAsync(existingProduct);
        mockProductService.Setup(x => x.UpdateAsync(It.IsAny<ProductDto>()))
            .ReturnsAsync(updatedProduct);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        mockLogger.Verify(x => x.LogInformation(It.Is<string>(msg => msg.Contains("Updating product"))), Times.Once);
        mockLogger.Verify(x => x.LogInformation(It.Is<string>(msg => msg.Contains("Product updated successfully"))), Times.Once);
        Assert.Equal(updatedProduct.Name, result.Name);
        Assert.Equal(updatedProduct.Description, result.Description);
    }

    [Fact]
    public async Task Handle_ShouldLogError_WhenExceptionOccurs()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 150,
            IsAvailable = false
        };

        // Simulating an exception when trying to get the product by ID
        mockProductService.Setup(x => x.GetByIdAsync(command.Id))
            .ThrowsAsync(new Exception("Test Exception"));

        // Act & Assert
        await Assert.ThrowsAsync<ApplicationException>(() => handler.Handle(command, CancellationToken.None));

        // Verifying that LogError is called with the correct message and exception
        mockLogger.Verify(
            x => x.LogError(It.Is<string>(msg => msg.Contains("Error occurred while updating the product")), 
                It.IsAny<Exception>()), // explicitly match the exception parameter
            Times.Once
        );
    }
}