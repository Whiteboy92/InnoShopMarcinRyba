using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;
using UserManagement.Application.Features.Users.Commands;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.EmailService;

namespace UserManagement.Application.Features.Users.Handlers;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, User>
{
    private readonly UserManager<User> userManager;
    private readonly IEmailService emailService;
    private readonly IValidator<CreateUserCommand> validator;
    private readonly ILogger<CreateUserCommandHandler> logger;

    public CreateUserCommandHandler(
        UserManager<User> userManager,
        IEmailService emailService,
        IValidator<CreateUserCommand> validator,
        ILogger<CreateUserCommandHandler> logger)
    {
        this.userManager = userManager;
        this.emailService = emailService;
        this.validator = validator;
        this.logger = logger;
    }

    public async Task<User> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting user creation process for Email: {Email}", request.Email);

        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            logger.LogWarning("Validation failed for Email: {Email}. Errors: {Errors}",
                request.Email,
                string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

            throw new ValidationException(validationResult.Errors);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            Name = request.Name,
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            logger.LogError("User creation failed for Email: {Email}. Errors: {Errors}",
                request.Email,
                string.Join("; ", result.Errors.Select(e => e.Description)));

            throw new IdentityException("User creation failed.", result.Errors);
        }

        var roleResult = await userManager.AddToRoleAsync(user, request.Role.ToString());
        if (!roleResult.Succeeded)
        {
            logger.LogError("Role assignment failed for Email: {Email}, Role: {Role}. Errors: {Errors}",
                request.Email,
                request.Role,
                string.Join("; ", roleResult.Errors.Select(e => e.Description)));

            throw new IdentityException("Role assignment failed.", roleResult.Errors);
        }

        logger.LogInformation("User created successfully with Email: {Email}, Role: {Role}", request.Email,
            request.Role);

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        await emailService.SendAccountVerificationEmailAsync(user.Email, token);

        logger.LogInformation("Account verification email sent to: {Email}", request.Email);

        return user;
    }
}