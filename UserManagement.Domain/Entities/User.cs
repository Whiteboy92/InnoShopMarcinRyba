using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using UserManagement.Domain.Enums;

namespace UserManagement.Domain.Entities;

[Table("Users")]
public class User : IdentityUser<Guid>
{
    [Required]
    [Column("Name")]
    public required string Name { get; set; }

    [Column("IsActive")] // soft delete flag
    public bool IsActive { get; set; } = true;
        
    [Column("Role")]
    public UserRole Role { get; set; }

    [Column("CreatedAt")] 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt")]
    public DateTime? UpdatedAt { get; set; }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}