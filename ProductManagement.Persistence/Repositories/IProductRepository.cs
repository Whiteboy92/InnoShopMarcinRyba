using ProductManagement.Domain.Entities;

namespace ProductManagement.Persistence.Repositories
{
    public interface IProductRepository
    {
        Task<List<Product>> GetProductsByUserIdAsync(Guid userId);
        Task<Product?> GetByIdAsync(Guid productId);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(Guid productId);
        Task SaveChangesAsync();
    }
}