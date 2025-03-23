using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace UserManagement.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<ExceptionHandlingMiddleware> logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred.");
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var problemDetails = new ProblemDetails
            {
                Title = "Internal Server Error",
                Status = (int)HttpStatusCode.InternalServerError,
                Detail = "An unexpected error occurred. Please try again later.",
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }
    }
}