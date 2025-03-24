using MediatR;
using ProductManagement.Application.DTOs;

namespace ProductManagement.Application.Features.Products.Commands;

public class UpdateProductCommand : IRequest<ProductDto>
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }
    public required bool IsAvailable { get; set; }
}