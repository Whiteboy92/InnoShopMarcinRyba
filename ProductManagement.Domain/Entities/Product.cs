using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductManagement.Domain.Entities;

[Table("Products")]
public class Product
{
    [Key]
    [Column("Id")]
    public Guid Id { get; set; }

    [Required]
    [Column("Name", TypeName = "nvarchar(200)")]
    public required string Name { get; set; }

    [Column("Description", TypeName = "nvarchar(1000)")]
    public required string? Description { get; set; }

    [Required]
    [Column("Price", TypeName = "decimal(18,2)")]
    public required decimal Price { get; set; }

    [Required]
    [Column("Availability")]
    public bool IsAvailable { get; set; }

    [Required]
    [Column("CreatorUserId")]
    public Guid CreatorUserId { get; set; }

    [Required]
    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [Column("UpdatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [Required]
    [Column("IsDeleted")]
    public bool IsDeleted { get; set; }
}