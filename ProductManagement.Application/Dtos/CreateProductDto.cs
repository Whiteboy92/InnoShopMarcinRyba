using System.ComponentModel.DataAnnotations;

namespace ProductManagement.Application.DTOs;

public class CreateProductDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Range(1.00, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    public bool IsAvailable { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
}