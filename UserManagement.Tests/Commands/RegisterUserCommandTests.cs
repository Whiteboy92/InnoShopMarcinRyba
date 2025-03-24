using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UserManagement.Application.Features.Users.Commands;
using UserManagement.Application.Features.Users.Handlers;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.EmailService;

namespace UserManagement.Tests.Commands;

public class RegisterCommandHandlerTests
{
    private readonly Mock<UserManager<User>> userManagerMock;
    private readonly Mock<IEmailService> emailServiceMock;
    private readonly Mock<IValidator<RegisterCommand>> validatorMock;
    private readonly Mock<ILogger<RegisterCommandHandler>> loggerMock;
    private readonly RegisterCommandHandler handler;

    public RegisterCommandHandlerTests()
    {
        userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<IPasswordHasher<User>>(),
            new List<IUserValidator<User>> { Mock.Of<IUserValidator<User>>() },
            new List<IPasswordValidator<User>> { Mock.Of<IPasswordValidator<User>>() },
            Mock.Of<ILookupNormalizer>(),
            Mock.Of<IdentityErrorDescriber>(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<User>>>()
        );
        
        emailServiceMock = new Mock<IEmailService>();
        validatorMock = new Mock<IValidator<RegisterCommand>>();
        loggerMock = new Mock<ILogger<RegisterCommandHandler>>();

        handler = new RegisterCommandHandler(
            userManagerMock.Object,
            emailServiceMock.Object,
            validatorMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldRegisterUser_WhenInputIsValid()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            Role = "User",
        };
        var validationResult = new ValidationResult();

        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(um => um.AddToRoleAsync(It.IsAny<User>(), "User"))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(um => um.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
            .ReturnsAsync("fake-token");
        emailServiceMock.Setup(es => es.SendAccountVerificationEmailAsync("test@example.com", "fake-token"))
            .Returns(Task.CompletedTask);

        var command = new RegisterCommand
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "Password123!",
            Role = "User",
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("User registered successfully with Email")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenInputIsInvalid()
    {
        // Arrange
        var validationErrors = new List<ValidationFailure>
        {
            new ValidationFailure("Email", "Invalid email format."),
        };
        var validationResult = new ValidationResult(validationErrors);

        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var command = new RegisterCommand
        {
            Name = "Test User",
            Email = "invalid-email",
            Password = "Password123!",
            Role = "User",
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Validation failed for Email")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }
}
