using FluentValidation;
using MediatR;
using UserManagement.Application.Features.Users.Commands;
using UserManagement.Application.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Application.Features.Users.Handlers;
using UserManagement.Application.Validators;
using UserManagement.Infrastructure.EmailService;
using UserManagement.Persistence.Repositories;
using Shared.Logging;

namespace UserManagement.API.Extensions;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IRequestHandler<CreateUserCommand, User>, CreateUserCommandHandler>();
            
        services.AddScoped<IValidator<CreateUserCommand>, CreateUserCommandValidator>();
            
        services.AddScoped<IEmailService, EmailService>();

        services.AddScoped<IUserRepository, UserRepository>();
    }

    public static void AddLoggingServices(this IServiceCollection services)
    {
        services.AddSingleton(typeof(ILoggerService<>), typeof(LoggerService<>));
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
    }
}