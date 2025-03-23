using FluentValidation;
using UserManagement.Application.Features.Users.Commands;
using UserManagement.Domain.Enums;

namespace UserManagement.Application.Validators;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters long.");
            
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(role => role is UserRole.Admin or UserRole.User).WithMessage("Role must be either 'Admin' or 'User'.");
    }
}