using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace UserApi.Infrastructures.ExceptionHandlers;

internal sealed class DbUpdateExceptionHandler(ILogger<DbUpdateExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not DbUpdateException dbUpdateException)
        {
            return false;
        }

        logger.LogError(dbUpdateException, "A database update exception occurred while processing the request.");

        var problemResult = dbUpdateException switch
        {
            UniqueConstraintException uniqueConstraintException => this.GetConflict(uniqueConstraintException),
            CannotInsertNullException cannotInsertNullException => this.GetNotAcceptable(cannotInsertNullException),
            _ => GetInternalServerError(dbUpdateException)
        };

        await problemResult.ExecuteAsync(httpContext);

        return true;
    }

    private ProblemHttpResult GetConflict(UniqueConstraintException exception)
    {
        return TypedResults.Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "A unique constraint violation occurred while processing the request.",
            detail: "A unique constraint violation occurred while processing the request. Please try again later.",
            extensions: new Dictionary<string, object?>
            {
                ["errors"] = exception.ConstraintProperties.ToDictionary(p => p, _ => new[] { exception.Message })
            }
        );
    }

    private ProblemHttpResult GetNotAcceptable(CannotInsertNullException exception)
    {
        return TypedResults.Problem(
            statusCode: StatusCodes.Status406NotAcceptable,
            title: "A null value insertion violation occurred while processing the request.",
            detail: "A null value insertion violation occurred while processing the request. Please try again later.",
            extensions: new Dictionary<string, object?>
            {
                ["errors"] = new[] { exception.Message }
            }
        );
    }

    private ProblemHttpResult GetInternalServerError(DbUpdateException exception)
    {
        return TypedResults.Problem(
            statusCode: StatusCodes.Status500InternalServerError,
            title: "An internal server error occurred while processing the request.",
            detail: "An internal server error occurred while processing the request. Please try again later.",
            extensions: new Dictionary<string, object?>
            {
                ["errors"] = new[] { exception.Message }
            }
        );
    }
}