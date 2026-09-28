using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SolidShop.Api.Common.Exceptions;

namespace SolidShop.Api.Common.Errors;

// Single Responsibility: the only place that converts exceptions into HTTP responses.
// Controllers and services never need try/catch for these cases.
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
            BusinessRuleException => (StatusCodes.Status400BadRequest, "Business rule violated"),
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception on {Path}", httpContext.Request.Path);

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception switch
            {
                ValidationException => "One or more validation errors occurred.",
                _ when statusCode == StatusCodes.Status500InternalServerError => "An unexpected error occurred.",
                _ => exception.Message
            },
            Instance = httpContext.Request.Path
        };

        if (exception is ValidationException validation)
        {
            problem.Extensions["errors"] = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }
}
