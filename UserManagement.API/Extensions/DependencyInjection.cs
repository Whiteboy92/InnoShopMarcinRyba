using FluentValidation;
using MediatR;
using UserManagement.Application.Features.Users.Commands;
using UserManagement.Application.Interfaces;
using UserManagement.Domain.Entities;
using Shared.Exceptions;
using UserManagement.Application.Validators;
using UserManagement.Infrastructure.EmailService;
using UserManagement.Persistence.Repositories;

namespace UserManagement.API.Extensions;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IRequestHandler<CreateUserCommand, User>, CreateUserCommandHandler>();
            
        services.AddScoped<IValidator<CreateUserCommand>, CreateUserCommandValidator>();
            
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<GlobalExceptionHandler>();

        services.AddScoped<IUserRepository, UserRepository>();
    }

    public static void AddLoggingServices(this IServiceCollection services)
    {
        services.AddScoped<ILogger<CreateUserCommandHandler>, Logger<CreateUserCommandHandler>>();
    }
}