using Shared.Exceptions;
using UserManagement.API.Extensions;
using UserManagement.API.Middleware;
using UserManagement.Infrastructure.Auth;

namespace UserManagement.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Register services in the container
        ConfigureServices(builder.Services);

        var app = builder.Build();

        // Configure middleware pipeline
        ConfigureMiddleware(app);

        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Add Authorization, Swagger, and Controller services
        services.AddAuthorization();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddControllers();

        // Register application-specific services
        services.AddApplicationServices();
        services.AddLoggingServices();

        // Register third-party services
        services.AddScoped<IJwtService, JwtService>();
        services.AddSingleton<GlobalExceptionHandler>();
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        app.UseMiddleware<GlobalExceptionHandler>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseHttpsRedirection();

        app.UseAuthorization();
    }
}