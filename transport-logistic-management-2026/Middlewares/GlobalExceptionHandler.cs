using business_logic.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ValidationException = FluentValidation.ValidationException;

namespace transport_logistic_management_2026.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var problemDetails = exception switch
            {
                ValidationException validationException => CreateValidationProblem(validationException),
                NotFoundException => CreateProblem(StatusCodes.Status404NotFound, "Not Found", exception.Message),
                BusinessRuleException => CreateProblem(StatusCodes.Status400BadRequest, "Bad Request", exception.Message),
                UnauthorizedAccessException => CreateProblem(StatusCodes.Status401Unauthorized, "Unauthorized", exception.Message),
                _ => CreateProblem(StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred.")
            };

            if (problemDetails.Status == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
            }
            else
            {
                _logger.LogWarning("Request failed with {Status}: {Message}", problemDetails.Status, exception.Message);
            }

            problemDetails.Instance = httpContext.Request.Path;
            httpContext.Response.StatusCode = problemDetails.Status!.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, problemDetails.GetType(), cancellationToken);

            return true;
        }

        private static ProblemDetails CreateProblem(int status, string title, string detail) => new()
        {
            Status = status,
            Title = title,
            Detail = detail
        };

        private static ValidationProblemDetails CreateValidationProblem(ValidationException exception)
        {
            var errors = exception.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred."
            };
        }
    }
}
