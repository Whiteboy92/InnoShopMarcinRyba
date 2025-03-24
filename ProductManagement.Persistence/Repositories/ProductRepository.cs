using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Persistence.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductDbContext context;
        private readonly ILogger<ProductRepository> logger;

        public ProductRepository(ProductDbContext context, ILogger<ProductRepository> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<List<Product>> GetProductsByUserIdAsync(Guid userId)
        {
            try
            {
                logger.LogInformation("Fetching products for user {UserId}", userId);
                return await context.Products
                    .AsNoTracking()
                    .Where(p => p.CreatorUserId == userId && !p.IsDeleted)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching products for user {UserId}", userId);
                throw;
            }
        }

        public async Task<Product?> GetByIdAsync(Guid productId)
        {
            try
            {
                logger.LogInformation("Fetching product with ID {ProductId}", productId);
                return await context.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching product with ID {ProductId}", productId);
                throw;
            }
        }

        public async Task AddAsync(Product product)
        {
            try
            {
                logger.LogInformation("Adding new product: {ProductName}", product.Name);
                await context.Products.AddAsync(product);
                await context.SaveChangesAsync();
                logger.LogInformation("Product {ProductName} added successfully", product.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding product: {ProductName}", product.Name);
                throw;
            }
        }

        public async Task UpdateAsync(Product product)
        {
            try
            {
                logger.LogInformation("Updating product with ID {ProductId}", product.Id);
                context.Products.Update(product);
                await context.SaveChangesAsync();
                logger.LogInformation("Product with ID {ProductId} updated successfully", product.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating product with ID {ProductId}", product.Id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid productId)
        {
            try
            {
                logger.LogInformation("Deleting product with ID {ProductId}", productId);
                var product = await context.Products.FindAsync(productId);

                if (product == null)
                {
                    logger.LogWarning("Product with ID {ProductId} not found", productId);
                    return;
                }

                product.IsDeleted = true;
                context.Products.Update(product);
                await context.SaveChangesAsync();

                logger.LogInformation("Product with ID {ProductId} deleted successfully", productId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting product with ID {ProductId}", productId);
                throw;
            }
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                logger.LogInformation("Saving changes to database.");
                await context.SaveChangesAsync();
                logger.LogInformation("Database changes saved successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving changes to database.");
                throw;
            }
        }
    }
}
