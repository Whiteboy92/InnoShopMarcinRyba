using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UserManagement.Application.Features.Users.Commands;
using UserManagement.Application.Features.Users.Handlers;
using UserManagement.Domain.Entities;

namespace UserManagement.Tests.Commands;

public class UpdateUserCommandHandlerTests
{
    private readonly Mock<UserManager<User>> userManagerMock;
    private readonly Mock<RoleManager<IdentityRole>> roleManagerMock;
    private readonly UpdateUserCommandHandler handler;

    public UpdateUserCommandHandlerTests()
    {
        // Mock UserManager
        userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<IPasswordHasher<User>>(),
            Array.Empty<IUserValidator<User>>(),
            Array.Empty<IPasswordValidator<User>>(),
            Mock.Of<ILookupNormalizer>(),
            Mock.Of<IdentityErrorDescriber>(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<User>>>()
        );

        // Mock RoleManager
        var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
        roleManagerMock = new Mock<RoleManager<IdentityRole>>(
            roleStoreMock.Object,
            Array.Empty<IRoleValidator<IdentityRole>>(),
            Mock.Of<ILookupNormalizer>(),
            Mock.Of<IdentityErrorDescriber>(),
            Mock.Of<ILogger<RoleManager<IdentityRole>>>()
        );

        // Mock Logger
        Mock<ILogger<UpdateUserCommandHandler>> loggerMock1 = new Mock<ILogger<UpdateUserCommandHandler>>();

        // Create handler instance
        handler = new UpdateUserCommandHandler(userManagerMock.Object, roleManagerMock.Object, loggerMock1.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateUserSuccessfully_WhenValidCommand()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Old Name",
            Email = "old@example.com",
            UserName = "old@example.com",
            Role = "User",
        };

        var command = new UpdateUserCommand
        {
            Id = userId,
            Name = "New Name",
            Email = "new@example.com",
            NewRole = "Admin",
        };

        // Mock UserManager behavior
        userManagerMock.Setup(um => um.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        userManagerMock.Setup(um => um.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });
        userManagerMock.Setup(um => um.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(um => um.AddToRoleAsync(user, command.NewRole))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(um => um.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Mock RoleManager behavior
        roleManagerMock.Setup(rm => rm.RoleExistsAsync(command.NewRole))
            .ReturnsAsync(true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        userManagerMock.Verify(um => um.UpdateAsync(user), Times.Once);
        userManagerMock.Verify(um => um.AddToRoleAsync(user, command.NewRole), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var command = new UpdateUserCommand
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@example.com",
        };

        userManagerMock.Setup(um => um.FindByIdAsync(command.Id.ToString()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() => handler.Handle(command, CancellationToken.None));

        Assert.Equal(404, exception.Details.Status);
        Assert.Equal("Not Found", exception.Details.Title);
        Assert.Equal($"User with Id {command.Id} not found.", exception.Details.Detail);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationError_WhenEmailIsInvalid()
    {
        // Arrange
        var command = new UpdateUserCommand
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "invalid-email",
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() => handler.Handle(command, CancellationToken.None));

        // Verify the status code, title, and detail
        Assert.Equal(400, exception.Details.Status);
        Assert.Equal("Validation Error", exception.Details.Title);
        Assert.Equal("Invalid email format.", exception.Details.Detail);
    }

    [Fact]
    public async Task Handle_ShouldThrowInternalServerError_WhenUnexpectedErrorOccurs()
    {
        // Arrange
        var command = new UpdateUserCommand
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@example.com",
        };

        userManagerMock.Setup(um => um.FindByIdAsync(command.Id.ToString()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ProblemDetailsException>(() => handler.Handle(command, CancellationToken.None));

        Assert.Equal(500, exception.Details.Status);
        Assert.Equal("Internal Server Error", exception.Details.Title);
        Assert.Equal("An unexpected error occurred.", exception.Details.Detail);
    }
}