using FluentValidation;
using UserManagement.Application.Features.Users.Commands;

namespace UserManagement.Application.Validators;

public class UserUpdateValidator : AbstractValidator<UpdateUserCommand>
{
    public UserUpdateValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters.");
    }
}