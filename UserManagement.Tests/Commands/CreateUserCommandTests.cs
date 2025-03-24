using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shared.Exceptions;
using UserManagement.Application.Features.Users.Commands;
using UserManagement.Application.Features.Users.Handlers;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.EmailService;

namespace UserManagement.Tests.Commands;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IValidator<CreateUserCommand>> _validatorMock;
    private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
    private readonly Mock<ILogger<CreateUserCommandHandler>> _loggerMock;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _userManagerMock = new Mock<UserManager<User>>(
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

        _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
            Mock.Of<IRoleStore<IdentityRole>>(),
            Array.Empty<IRoleValidator<IdentityRole>>(),
            Mock.Of<ILookupNormalizer>(),
            Mock.Of<IdentityErrorDescriber>(),
            Mock.Of<ILogger<RoleManager<IdentityRole>>>()
        );

        _emailServiceMock = new Mock<IEmailService>();
        _validatorMock = new Mock<IValidator<CreateUserCommand>>();
        _loggerMock = new Mock<ILogger<CreateUserCommandHandler>>();

        _handler = new CreateUserCommandHandler(
            _userManagerMock.Object,
            _roleManagerMock.Object,
            _emailServiceMock.Object,
            _validatorMock.Object,
            _loggerMock.Object
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
            Role = "User",
        };

        _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        
        _roleManagerMock.Setup(rm => rm.RoleExistsAsync(command.Role))
            .ReturnsAsync(true)
            .Verifiable();

        _userManagerMock.Setup(um => um.CreateAsync(It.Is<User>(u => u.Email == command.Email), command.Password))
            .ReturnsAsync(IdentityResult.Success)
            .Verifiable();

        _userManagerMock.Setup(um => um.AddToRoleAsync(It.Is<User>(u => u.Email == command.Email), command.Role))
            .ReturnsAsync(IdentityResult.Success)
            .Verifiable();

        _userManagerMock.Setup(um => um.GenerateEmailConfirmationTokenAsync(It.Is<User>(u => u.Email == command.Email)))
            .ReturnsAsync("mock-token")
            .Verifiable();

        _emailServiceMock.Setup(es => es.SendAccountVerificationEmailAsync(command.Email, "mock-token"))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(command.Email, result.Email);
        
        // Verify all setups were called
        _roleManagerMock.Verify();
        _userManagerMock.Verify();
        _emailServiceMock.Verify();
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenRequestIsInvalid()
    {
        // Arrange
        var command = new CreateUserCommand 
        { 
            Name = "", 
            Email = "invalid", 
            Password = "123", 
            Role = "InvalidRole",
        };

        var validationFailures = new List<ValidationFailure> 
        { 
            new("Email", "Invalid email format."),
            new("Role", "Invalid role specified."),
        };
        
        _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
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
            Role = "User",
        };

        _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
            
        _roleManagerMock.Setup(rm => rm.RoleExistsAsync(command.Role))
            .ReturnsAsync(true);
            
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), command.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "User creation failed." }));

        // Act & Assert
        await Assert.ThrowsAsync<IdentityException>(() => _handler.Handle(command, CancellationToken.None));
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
            Role = "User",
        };

        _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
            
        _roleManagerMock.Setup(rm => rm.RoleExistsAsync(command.Role))
            .ReturnsAsync(true);
            
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), command.Password))
            .ReturnsAsync(IdentityResult.Success);
            
        _userManagerMock.Setup(um => um.AddToRoleAsync(It.IsAny<User>(), command.Role))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role assignment failed." }));

        // Act & Assert
        await Assert.ThrowsAsync<IdentityException>(() => _handler.Handle(command, CancellationToken.None));
    }
}