using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserApi.Infrastructures.Extensions;

namespace UserApi.Features.Users.Find;

public static class FindUserEndpoint
{
    public static IEndpointRouteBuilder MapFindUserEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/users", async (
            [AsParameters] FindUserPayload payload, 
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new FindUserRequest(payload), cancellationToken);
            return result.ToTypedResults(user => TypedResults.Ok(user));
        })
        .WithName("FindUser")
        .WithTags("Users")
        .Produces<FindUserResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        return app;
    }
}