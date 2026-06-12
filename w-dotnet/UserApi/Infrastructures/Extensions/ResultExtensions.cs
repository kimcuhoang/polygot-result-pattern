using UserApi.Models;

namespace UserApi.Infrastructures.Extensions;

public static class ResultExtensions
{
    public static IResult ToTypedResults(this Result result, Func<IResult> onSuccess)
    {
        if (result.IsSuccess)
        {
            return onSuccess();
        }
        
        return result.Reason!.Type switch
        {
            ReasonType.NotFound => TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, detail: result.Reason.Message),
            ReasonType.Forbidden => TypedResults.Problem(statusCode: StatusCodes.Status403Forbidden, detail: result.Reason.Message),
            ReasonType.Conflict => TypedResults.Problem(statusCode: StatusCodes.Status409Conflict, detail: result.Reason.Message),
            ReasonType.NotAcceptable => TypedResults.Problem(statusCode: StatusCodes.Status406NotAcceptable, detail: result.Reason.Message),
            _ => TypedResults.Problem(statusCode: StatusCodes.Status500InternalServerError, detail: "An unexpected error occurred.")
        };
    }

    public static IResult ToTypedResults<T>(this Result<T> result, Func<T, IResult> onSuccess)
    {
        if (result.IsSuccess)
        {
            return onSuccess(result.Value!);
        }
            

        return result.Reason!.Type switch
        {
            ReasonType.NotFound => TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, detail: result.Reason.Message),
            ReasonType.Forbidden => TypedResults.Problem(statusCode: StatusCodes.Status403Forbidden, detail: result.Reason.Message),
            ReasonType.Conflict => TypedResults.Problem(statusCode: StatusCodes.Status409Conflict, detail: result.Reason.Message),
            ReasonType.NotAcceptable => TypedResults.Problem(statusCode: StatusCodes.Status406NotAcceptable, detail: result.Reason.Message),
            _ => TypedResults.Problem(statusCode: StatusCodes.Status500InternalServerError, detail: "An unexpected error occurred.")
        };
    }
}