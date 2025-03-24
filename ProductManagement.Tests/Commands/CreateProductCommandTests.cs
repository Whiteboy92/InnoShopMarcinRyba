using Moq;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Features.Products.Commands;
using ProductManagement.Application.Features.Products.Handlers;
using ProductManagement.Application.Interfaces;
using Shared.Logging;

namespace ProductManagement.Tests.Commands;

public class CreateProductHandlerTests
{
    private readonly Mock<IProductService> mockProductService;
    private readonly Mock<ILoggerService<CreateProductHandler>> mockLogger;
    private readonly CreateProductHandler handler;

    public CreateProductHandlerTests()
    {
        mockProductService = new Mock<IProductService>();
        mockLogger = new Mock<ILoggerService<CreateProductHandler>>();
        handler = new CreateProductHandler(mockProductService.Object, mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateProductSuccessfully()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Description",
            Price = 100,
        };

        var createdProduct = new ProductDto
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Description = command.Description,
            Price = command.Price,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };

        mockProductService.Setup(x => x.CreateAsync(It.IsAny<CreateProductDto>()))
            .ReturnsAsync(new ProductDto
            {
                Id = createdProduct.Id,
                Name = createdProduct.Name,
                Description = createdProduct.Description,
                Price = createdProduct.Price,
                IsAvailable = createdProduct.IsAvailable,
                CreatedAt = createdProduct.CreatedAt
            });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        mockLogger.Verify(x => x.LogInformation(It.Is<string>(msg => msg.Contains("Creating a new product"))), Times.Once);
        mockLogger.Verify(x => x.LogInformation(It.Is<string>(msg => msg.Contains("Product created successfully"))), Times.Once);
        Assert.Equal(createdProduct.Id, result.Id);
        Assert.Equal(createdProduct.Name, result.Name);
        Assert.Equal(createdProduct.Description, result.Description);
    }

    [Fact]
    public async Task Handle_ShouldLogError_WhenExceptionOccurs()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Description",
            Price = 100,
        };

        // Simulating an exception in the service
        mockProductService.Setup(x => x.CreateAsync(It.IsAny<CreateProductDto>()))
            .ThrowsAsync(new Exception("Test Exception"));

        // Act & Assert
        await Assert.ThrowsAsync<ApplicationException>(() => handler.Handle(command, CancellationToken.None));

        // Verifying that the LogError method is called with the correct message and exception
        mockLogger.Verify(
            x => x.LogError(It.Is<string>(msg => msg.Contains("Error occurred while creating the product")), 
                It.IsAny<Exception>()), // explicitly match the exception parameter
            Times.Once
        );
    }
}