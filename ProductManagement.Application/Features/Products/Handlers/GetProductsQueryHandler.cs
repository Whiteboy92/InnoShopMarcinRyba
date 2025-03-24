using Microsoft.EntityFrameworkCore;
using ProductManagement.Application.Features.Products.Queries;
using ProductManagement.Application.Interfaces;
using ProductManagement.Domain.Entities;
using ProductManagement.Persistence;

namespace ProductManagement.Application.Features.Products.Handlers
{
    public class GetProductsQueryHandler : IGetProductsQueryHandler
    {
        private readonly ProductDbContext dbContext;

        public GetProductsQueryHandler(ProductDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<Product>> HandleAsync(ProductQuery query)
        {
            var products = dbContext.Products.AsQueryable();

            // product name
            if (!string.IsNullOrEmpty(query.Name))
            {
                products = products.Where(p => p.Name.Contains(query.Name));
            }

            // minimum price
            if (query.MinPrice.HasValue)
            {
                products = products.Where(p => p.Price >= query.MinPrice);
            }

            // maximum price
            if (query.MaxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= query.MaxPrice);
            }
            
            products = products.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize);

            return await products.ToListAsync();
        }
    }
}