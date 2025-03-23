using Microsoft.AspNetCore.Identity;
using UserManagement.Application.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Auth;

namespace UserManagement.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> userManager;
    private readonly SignInManager<User> signInManager;
    private readonly IJwtService jwtService;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtService jwtService)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.jwtService = jwtService;
    }

    public async Task<string> LoginAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, password, false);
        if (!result.Succeeded)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var roles = await userManager.GetRolesAsync(user);

        var token = jwtService.GenerateToken(user.Id, user.Email, roles);
        return token;
    }
}