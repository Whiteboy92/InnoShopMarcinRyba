using UserManagement.Domain.Enums;

namespace UserManagement.Application.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string? Email { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
}