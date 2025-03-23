namespace UserManagement.Infrastructure.Auth;

public interface IJwtService
{
    string GenerateToken(Guid userId, string? email, IEnumerable<string> roles);
}