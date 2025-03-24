using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shared.Exceptions;
using System.Text;
using UserManagement.API.Extensions;
using UserManagement.Application.Features.Users.Commands;
using UserManagement.Application.Features.Users.Handlers;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Validators;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Auth;
using UserManagement.Infrastructure.EmailService;
using UserManagement.Persistence;
using UserManagement.Persistence.Repositories;

namespace UserManagement.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureServices(builder.Services, builder.Configuration);

        var app = builder.Build();

        ConfigureMiddleware(app);

        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Add controllers and API explorer
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Configure DbContext with Identity
        services.AddDbContext<UserDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("UsersDb"),
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(UserDbContext).Assembly.FullName)
            ));

        // Configure Identity with custom options
        services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 1;

                options.User.RequireUniqueEmail = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<UserDbContext>()
            .AddDefaultTokenProviders();

        // Configure JWT Authentication
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
            options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User"));
        });
        
        services.AddApplicationServices();
        services.AddLoggingServices();
        
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IEmailService, EmailService>();
        
        services.AddScoped<IRequestHandler<CreateUserCommand, User>, CreateUserCommandHandler>();
        
        services.AddScoped<IValidator<CreateUserCommand>, CreateUserCommandValidator>();
        
        services.AddScoped<IUserRepository, UserRepository>();
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseDeveloperExceptionPage();
        }

        // Global error handling
        app.UseMiddleware<GlobalExceptionHandler>();

        // HTTPS redirection
        app.UseHttpsRedirection();

        // Routing
        app.UseRouting();

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Health check endpoint
        app.MapHealthChecks("/health");

        // Map controllers
        app.MapControllers();

        // Database migration (in development)
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<UserDbContext>();
                var userManager = services.GetRequiredService<UserManager<User>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while migrating or seeding the database.");
            }
        }
    }
}