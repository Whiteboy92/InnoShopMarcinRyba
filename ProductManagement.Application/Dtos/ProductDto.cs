namespace ProductManagement.Application.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; } = string.Empty;
    public required string? Description { get; set; }
    public required decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
}