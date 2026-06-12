using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace UserApi.Infrastructures.ExceptionHandlers;

internal sealed class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
        {
            return false;
        }

        logger.LogError(exception, "A validation exception occurred while processing the request.");

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        var errors = validationException.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                failures => failures.Key,
                failures => failures.Select(f => f.ErrorMessage).ToArray()
            );

        var problemDetails = TypedResults.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            detail: "One or more validation errors occurred. Please refer to the errors property for additional details.",
            title: "Validation Error",
            extensions: new Dictionary<string, object?>
            {
                ["errors"] = errors
            }
        );

        await problemDetails.ExecuteAsync(httpContext);

        return true;
    }
}