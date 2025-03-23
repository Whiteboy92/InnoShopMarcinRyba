using System.Security.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UserManagement.Application.Features.Users.Commands;
using UserManagement.Application.Features.Users.Handlers;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Auth;

namespace UserManagement.Tests.Commands;

public class LoginUserCommandHandlerTests
{
    private readonly Mock<UserManager<User>> userManagerMock;
    private readonly Mock<IJwtService> jwtServiceMock;
    private readonly TestLogger<LoginUserCommandHandler> testLogger;
    private readonly LoginUserCommandHandler handler;

    public LoginUserCommandHandlerTests()
    {
        userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<IPasswordHasher<User>>().Object,
            Array.Empty<IUserValidator<User>>(),
            Array.Empty<IPasswordValidator<User>>(),
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<User>>>().Object);

        jwtServiceMock = new Mock<IJwtService>();
        testLogger = new TestLogger<LoginUserCommandHandler>();

        handler = new LoginUserCommandHandler(
            userManagerMock.Object,
            jwtServiceMock.Object,
            testLogger);
    }

    [Fact]
    public async Task Handle_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
        };
        var roles = new List<string> { "User" };
        var token = "valid-jwt-token";

        userManagerMock.Setup(um => um.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);
        userManagerMock.Setup(um => um.CheckPasswordAsync(user, "password"))
            .ReturnsAsync(true);
        userManagerMock.Setup(um => um.GetRolesAsync(user))
            .ReturnsAsync(roles);
        jwtServiceMock.Setup(js => js.GenerateToken(user.Id, user.Email, roles))
            .Returns(token);

        var command = new LoginUserCommand { Email = "test@example.com", Password = "password" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(token, result);
        Assert.Contains("User test@example.com successfully logged in.", testLogger.LogMessages);
    }

    [Fact]
    public async Task Handle_ShouldThrowAuthenticationException_WhenUserDoesNotExist()
    {
        // Arrange
        userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var command = new LoginUserCommand { Email = "invalid@example.com", Password = "password" };

        // Act & Assert
        await Assert.ThrowsAsync<AuthenticationException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("Failed login attempt for email: invalid@example.com", testLogger.LogMessages);
    }

    [Fact]
    public async Task Handle_ShouldThrowAuthenticationException_WhenPasswordIsInvalid()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
        };

        userManagerMock.Setup(um => um.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);
        userManagerMock.Setup(um => um.CheckPasswordAsync(user, It.IsAny<string>()))
            .ReturnsAsync(false);

        var command = new LoginUserCommand { Email = "test@example.com", Password = "wrong-password" };

        // Act & Assert
        await Assert.ThrowsAsync<AuthenticationException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("Failed login attempt for email: test@example.com", testLogger.LogMessages);
    }
}
