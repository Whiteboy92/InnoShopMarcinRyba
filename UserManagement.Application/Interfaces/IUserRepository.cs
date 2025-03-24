using UserManagement.Domain.Entities;

namespace UserManagement.Application.Interfaces;

public interface IUserRepository
{
    Task<User> GetByIdAsync(Guid userId);
    Task<List<User>> GetAllAsync();
    Task<bool> CreateAsync(User user);
    Task<bool> UpdateAsync(User user);
    Task<bool> DeleteAsync(Guid userId);
    Task<User> GetByEmailAsync(string userEmail);
}