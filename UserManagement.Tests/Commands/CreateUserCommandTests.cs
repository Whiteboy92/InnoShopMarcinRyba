using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shared.Exceptions;
using UserManagement.Application.Features.Users.Commands;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;
using UserManagement.Infrastructure.EmailService;

namespace UserManagement.Tests.Commands;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<UserManager<User>> userManagerMock;
    private readonly Mock<IEmailService> emailServiceMock;
    private readonly Mock<IValidator<CreateUserCommand>> validatorMock;
    private readonly CreateUserCommandHandler handler;

    public CreateUserCommandHandlerTests()
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

        emailServiceMock = new Mock<IEmailService>();
        validatorMock = new Mock<IValidator<CreateUserCommand>>();
        var loggerMock1 = new Mock<ILogger<CreateUserCommandHandler>>();

        handler = new CreateUserCommandHandler(
            userManagerMock.Object,
            emailServiceMock.Object,
            validatorMock.Object,
            loggerMock1.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateUser_WhenRequestIsValid()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "Password123!",
            Role = UserRole.User,
        };

        validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), command.Password))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(um => um.AddToRoleAsync(It.IsAny<User>(), command.Role.ToString()))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(um => um.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
            .ReturnsAsync("mock-token");
        emailServiceMock.Setup(es => es.SendAccountVerificationEmailAsync(command.Email, "mock-token"))
            .Returns(Task.CompletedTask);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(command.Email, result.Email);
        userManagerMock.Verify(um => um.CreateAsync(It.IsAny<User>(), command.Password), Times.Once);
        userManagerMock.Verify(um => um.AddToRoleAsync(It.IsAny<User>(), command.Role.ToString()), Times.Once);
        emailServiceMock.Verify(es => es.SendAccountVerificationEmailAsync(command.Email, "mock-token"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenRequestIsInvalid()
    {
        // Arrange
        var command = new CreateUserCommand { Name = "", Email = "invalid", Password = "123", Role = UserRole.User };

        var validationFailures = new List<ValidationFailure> { new("Email", "Invalid email format.") };
        validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowIdentityException_WhenUserCreationFails()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "Password123!",
            Role = UserRole.User,
        };

        validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), command.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "User creation failed." }));

        // Act & Assert
        await Assert.ThrowsAsync<IdentityException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowIdentityException_WhenRoleAssignmentFails()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "Password123!",
            Role = UserRole.User,
        };

        validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), command.Password))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(um => um.AddToRoleAsync(It.IsAny<User>(), command.Role.ToString()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role assignment failed." }));

        // Act & Assert
        await Assert.ThrowsAsync<IdentityException>(() => handler.Handle(command, CancellationToken.None));
    }
}