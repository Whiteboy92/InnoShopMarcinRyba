using MediatR;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Features.Products.Commands;
using ProductManagement.Application.Interfaces;
using Shared.Logging; // Assuming this namespace contains the logger

namespace ProductManagement.Application.Features.Products.Handlers;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IProductService productService;
    private readonly ILoggerService<UpdateProductHandler> logger;

    public UpdateProductHandler(IProductService productService, ILoggerService<UpdateProductHandler> logger)
    {
        this.productService = productService;
        this.logger = logger;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation($"Updating product with ID: {request.Id}");

            var existingProduct = await productService.GetByIdAsync(request.Id);
            if (existingProduct == null)
            {
                logger.LogWarning($"Product with ID: {request.Id} not found.");
                throw new KeyNotFoundException("Product not found.");
            }

            existingProduct.Name = request.Name;
            existingProduct.Description = request.Description;
            existingProduct.Price = request.Price;
            existingProduct.IsAvailable = request.IsAvailable;

            var updatedProduct = await productService.UpdateAsync(existingProduct);

            logger.LogInformation($"Product updated successfully with ID: {request.Id}");

            return new ProductDto
            {
                Id = updatedProduct.Id,
                Name = updatedProduct.Name,
                Description = updatedProduct.Description,
                Price = updatedProduct.Price,
                IsAvailable = updatedProduct.IsAvailable,
                CreatedAt = updatedProduct.CreatedAt,
            };
        }
        catch (Exception ex)
        {
            logger.LogError("Error occurred while updating the product.");
            throw new ApplicationException("An error occurred while updating the product.", ex);
        }
    }
}
