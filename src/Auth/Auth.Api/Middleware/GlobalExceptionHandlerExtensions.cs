using Auth.Application.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Middleware;

public static class GlobalExceptionHandlerExtensions
{
    public static void UseGlobalExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = exceptionHandlerFeature?.Error;

                if (exception is not null and not ValidationException and not InvalidCredentialsException)
                {
                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError(exception, "Excepción no controlada procesando {Path}", context.Request.Path);
                }

                var (statusCode, title, errors) = exception switch
                {
                    ValidationException validationException => (
                        StatusCodes.Status400BadRequest,
                        "Uno o más errores de validación ocurrieron.",
                        validationException.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())),
                    InvalidCredentialsException => (
                        StatusCodes.Status401Unauthorized,
                        exception.Message,
                        null),
                    _ => (
                        StatusCodes.Status500InternalServerError,
                        "Ocurrió un error inesperado al procesar la solicitud.",
                        null)
                };

                var problemDetails = new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Extensions =
                    {
                        ["traceId"] = context.TraceIdentifier,
                        ["errors"] = errors
                    }
                };

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(problemDetails);
            });
        });
    }
}
