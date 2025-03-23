using System.Security.Authentication;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Features.Users.Commands;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Auth;

namespace UserManagement.Application.Features.Users.Handlers;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, string>
{
    private readonly UserManager<User> userManager;
    private readonly IJwtService jwtService;
    private readonly ILogger<LoginUserCommandHandler> logger;

    public LoginUserCommandHandler(UserManager<User> userManager, IJwtService jwtService, ILogger<LoginUserCommandHandler> logger)
    {
        this.userManager = userManager;
        this.jwtService = jwtService;
        this.logger = logger;
    }

    public async Task<string> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
            throw new AuthenticationException("Invalid email or password.");
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = jwtService.GenerateToken(user.Id, user.Email, roles);

        logger.LogInformation("User {Email} successfully logged in.", request.Email);
        return token;
    }
}