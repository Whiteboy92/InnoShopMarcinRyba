using MediatR;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Features.Products.Commands;
using ProductManagement.Application.Interfaces;
using Shared.Logging; // Assuming this namespace contains the logger

namespace ProductManagement.Application.Features.Products.Handlers;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductService productService;
    private readonly ILoggerService<CreateProductHandler> logger;

    public CreateProductHandler(IProductService productService, ILoggerService<CreateProductHandler> logger)
    {
        this.productService = productService;
        this.logger = logger;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation($"Creating a new product with name: {request.Name}");

            var product = new CreateProductDto
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow,
            };

            var createdProduct = await productService.CreateAsync(product);
            
            logger.LogInformation($"Product created successfully with ID: {request.Id}");

            return new ProductDto
            {
                Id = createdProduct.Id,
                Name = createdProduct.Name,
                Description = createdProduct.Description,
                Price = createdProduct.Price,
                IsAvailable = createdProduct.IsAvailable,
                CreatedAt = createdProduct.CreatedAt,
            };
        }
        catch (Exception ex)
        {
            logger.LogError("Error occurred while creating the product.");
            throw new ApplicationException("An error occurred while creating the product.", ex);
        }
    }
}
