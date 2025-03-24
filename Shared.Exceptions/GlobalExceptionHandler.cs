using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Infrastructure.EmailService;

namespace Shared.Exceptions;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate next;
    private readonly ILogger<GlobalExceptionHandler> logger;

    public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, problemDetails) = exception switch
        {
            ValidationException validationException => (
                (int)HttpStatusCode.BadRequest,
                new ProblemDetails
                {
                    Title = "Validation error",
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = string.Join("; ", validationException.Errors.Select(e => e.ErrorMessage)),
                }),

            IdentityException identityException => (
                (int)HttpStatusCode.BadRequest,
                new ProblemDetails
                {
                    Title = "Identity error",
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = string.Join("; ", identityException.Errors.Select(e => e.Description)),
                }),

            UnauthorizedAccessException => (
                (int)HttpStatusCode.Unauthorized,
                new ProblemDetails
                {
                    Title = "Unauthorized",
                    Status = (int)HttpStatusCode.Unauthorized,
                    Detail = "You do not have permission to access this resource.",
                }),

            KeyNotFoundException => (
                (int)HttpStatusCode.NotFound,
                new ProblemDetails
                {
                    Title = "Resource not found",
                    Status = (int)HttpStatusCode.NotFound,
                    Detail = "The requested resource was not found.",
                }),

            EmailSendException emailSendException => (
                (int)HttpStatusCode.InternalServerError,
                new ProblemDetails
                {
                    Title = "Email Sending Failed",
                    Status = (int)HttpStatusCode.InternalServerError,
                    Detail = emailSendException.Message,
                }),

            ArgumentException or ArgumentNullException => (
                (int)HttpStatusCode.BadRequest,
                new ProblemDetails
                {
                    Title = "Invalid argument",
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = exception.Message,
                }),

            NotImplementedException => (
                (int)HttpStatusCode.NotImplemented,
                new ProblemDetails
                {
                    Title = "Not implemented",
                    Status = (int)HttpStatusCode.NotImplemented,
                    Detail = "The requested functionality is not implemented.",
                }),

            TimeoutException => (
                (int)HttpStatusCode.GatewayTimeout,
                new ProblemDetails
                {
                    Title = "Request timeout",
                    Status = (int)HttpStatusCode.GatewayTimeout,
                    Detail = "The request timed out. Please try again later.",
                }),

            _ => (
                (int)HttpStatusCode.InternalServerError,
                new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Status = (int)HttpStatusCode.InternalServerError,
                    Detail = "An unexpected error occurred. Please try again later.",
                }),
        };

        logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }
}
