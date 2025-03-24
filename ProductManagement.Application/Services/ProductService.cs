using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Mapping;
using ProductManagement.Domain.Entities;
using ProductManagement.Persistence;
using Shared.Logging;
using Shared.Exceptions;

namespace ProductManagement.Application.Services;

public class ProductService : IProductService
{
    private readonly ProductDbContext dbContext;
    private readonly ILoggerService<ProductService> logger;

    public ProductService(ProductDbContext dbContext, ILoggerService<ProductService> logger)
    {
        this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        try
        {
            logger.LogInformation("Fetching all products.");
            var products = await dbContext.Products
                .AsNoTracking()
                .ToListAsync();

            var productDtos = products.Select(p => p.ToDto()).ToList();

            logger.LogInformation($"Successfully fetched {productDtos.Count} products.");

            return productDtos;
        }
        catch (Exception ex)
        {
            logger.LogError($"An error occurred while fetching all products: {ex.Message}", ex);
            throw new IdentityException("An error occurred while fetching the products.",
                [new IdentityError { Description = ex.Message }]);
        }
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        try
        {
            logger.LogInformation($"Fetching product with ID: {id}");
            var product = await dbContext.Products.FindAsync(id);

            if (product == null)
            {
                logger.LogWarning($"Product with ID {id} not found.");
                throw new KeyNotFoundException($"Product with ID {id} not found.");
            }

            logger.LogInformation($"Successfully fetched product: {product.Name} ({product.Id})");

            return product.ToDto();
        }
        catch (KeyNotFoundException knfEx)
        {
            logger.LogWarning(knfEx.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError($"An error occurred while fetching the product with ID {id}: {ex.Message}", ex);
            throw new IdentityException($"An error occurred while fetching the product with ID {id}.",
                [new IdentityError { Description = ex.Message }]);
        }
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        try
        {
            logger.LogInformation("Creating a new product.");
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                IsAvailable = dto.IsAvailable,
                CreatedAt = DateTime.UtcNow,
            };

            await dbContext.Products.AddAsync(product);
            await dbContext.SaveChangesAsync();

            logger.LogInformation($"Product created: {product.Name} ({product.Id})");

            return product.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError($"An error occurred while creating a new product: {ex.Message}", ex);
            throw new IdentityException("An error occurred while creating the product.", 
                [new IdentityError { Description = ex.Message }]);
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            logger.LogInformation($"Attempting to delete product with ID: {id}");
            var product = await dbContext.Products.FindAsync(id);
            if (product == null)
            {
                logger.LogWarning($"Product with ID {id} not found. Deletion failed.");
                throw new KeyNotFoundException($"Product with ID {id} not found. Deletion failed.");
            }

            dbContext.Products.Remove(product);
            await dbContext.SaveChangesAsync();

            logger.LogInformation($"Product deleted: {product.Name} ({product.Id})");
            return true;
        }
        catch (KeyNotFoundException knfEx)
        {
            logger.LogWarning(knfEx.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError($"An error occurred while deleting the product with ID {id}: {ex.Message}", ex);
            throw new IdentityException($"An error occurred while deleting the product with ID {id}.",
                [new IdentityError { Description = ex.Message }]);
        }
    }

    public async Task<ProductDto> UpdateAsync(ProductDto updatedProduct)
    {
        try
        {
            logger.LogInformation($"Updating product with ID: {updatedProduct.Id}");
            var product = await dbContext.Products.FindAsync(updatedProduct.Id);
            if (product == null)
            {
                logger.LogWarning($"Product with ID {updatedProduct.Id} not found. Update failed.");
                throw new KeyNotFoundException($"Product with ID {updatedProduct.Id} not found. Update failed.");
            }

            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;
            product.IsAvailable = updatedProduct.IsAvailable;

            dbContext.Products.Update(product);
            await dbContext.SaveChangesAsync();

            logger.LogInformation($"Product updated: {product.Name} ({product.Id})");

            return product.ToDto();
        }
        catch (KeyNotFoundException knfEx)
        {
            logger.LogWarning(knfEx.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError($"An error occurred while updating the product with ID {updatedProduct.Id}: {ex.Message}", ex);
            throw new IdentityException($"An error occurred while updating the product with ID {updatedProduct.Id}.",
                [new IdentityError { Description = ex.Message }]);
        }
    }
}
