using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UserManagement.Application.Features.Users.Commands;
using UserManagement.Domain.Entities;

namespace UserManagement.Tests.Commands;

public class DeleteUserCommandHandlerTests
{
    private readonly Mock<UserManager<User>> userManagerMock;
    private readonly Mock<ILogger<DeleteUserCommandHandler>> loggerMock;
    private readonly DeleteUserCommandHandler handler;

    public DeleteUserCommandHandlerTests()
    {
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

        loggerMock = new Mock<ILogger<DeleteUserCommandHandler>>();
        handler = new DeleteUserCommandHandler(userManagerMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        var command = new DeleteUserCommand(Guid.NewGuid());
        userManagerMock.Setup(um => um.FindByIdAsync(command.Id.ToString()))
            .ReturnsAsync((User?)null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result);
        userManagerMock.Verify(um => um.DeleteAsync(It.IsAny<User>()), Times.Never);
        loggerMock.Verify(log => log.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnTrue_WhenUserIsSuccessfullyDeleted()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
        };
        var command = new DeleteUserCommand(user.Id);

        userManagerMock.Setup(um => um.FindByIdAsync(command.Id.ToString()))
            .ReturnsAsync(user);
        userManagerMock.Setup(um => um.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result);
        userManagerMock.Verify(um => um.DeleteAsync(user), Times.Once);
        loggerMock.Verify(log => log.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenUserDeletionFails()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
        };
        var command = new DeleteUserCommand(user.Id);

        var identityErrors = new[]
        {
            new IdentityError { Description = "Deletion error" },
        };

        userManagerMock.Setup(um => um.FindByIdAsync(command.Id.ToString()))
            .ReturnsAsync(user);
        userManagerMock.Setup(um => um.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result);
        userManagerMock.Verify(um => um.DeleteAsync(user), Times.Once);
        loggerMock.Verify(log => log.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUnexpectedErrorOccurs()
    {
        var command = new DeleteUserCommand(Guid.NewGuid());

        userManagerMock.Setup(um => um.FindByIdAsync(command.Id.ToString()))
            .ThrowsAsync(new Exception("Unexpected error"));

        await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));

        loggerMock.Verify(log => log.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}