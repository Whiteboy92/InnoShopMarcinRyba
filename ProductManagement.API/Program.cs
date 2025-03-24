using Microsoft.EntityFrameworkCore;
using ProductManagement.API.Extensions;
using ProductManagement.API.Middleware;
using ProductManagement.Persistence;
using Shared.Exceptions;

namespace ProductManagement.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Register services
        ConfigureServices(builder.Services, builder.Configuration);

        var app = builder.Build();

        // Configure middleware pipeline
        ConfigureMiddleware(app);

        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthorization();
        services.AddApplicationServices();
        services.AddLoggingServices();

        // Register controllers
        services.AddControllers();

        // Swagger for API documentation
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Database context
        services.AddDbContext<ProductDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("ProductDb")));
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

        // Ensure controllers are mapped
        app.MapControllers();
    }
}