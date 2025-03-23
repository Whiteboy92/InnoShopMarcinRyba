using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;
using UserManagement.Application.Features.Users.Commands;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.EmailService;

namespace UserManagement.Application.Features.Users.Handlers;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, User>
{
    private readonly UserManager<User> userManager;
    private readonly IEmailService emailService;
    private readonly IValidator<RegisterCommand> validator;
    private readonly ILogger<RegisterCommandHandler> logger;

    public RegisterCommandHandler(
        UserManager<User> userManager,
        IEmailService emailService,
        IValidator<RegisterCommand> validator,
        ILogger<RegisterCommandHandler> logger)
    {
        this.userManager = userManager;
        this.emailService = emailService;
        this.validator = validator;
        this.logger = logger;
    }

    public async Task<User> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting user registration process for Email: {Email}", request.Email);

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
            // Pass the IdentityError collection into the IdentityException
            throw new IdentityException("User registration failed.", result.Errors);
        }
    
        var roleResult = await userManager.AddToRoleAsync(user, request.Role);
        if (!roleResult.Succeeded)
        {
            // Pass the IdentityError collection into the IdentityException
            throw new IdentityException("Role assignment failed.", roleResult.Errors);
        }

        logger.LogInformation("User registered successfully with Email: {Email}, Role: {Role}", request.Email, request.Role);

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        await emailService.SendAccountVerificationEmailAsync(user.Email, token);

        logger.LogInformation("Account verification email sent to: {Email}", request.Email);

        return user;
    }
}
