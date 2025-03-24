using ProductManagement.Application.Features.Products.Queries;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Interfaces
{
    public interface IGetProductsQueryHandler
    {
        Task<List<Product>> HandleAsync(ProductQuery query);
    }
}