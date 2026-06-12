using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserApi.Infrastructures.Extensions;

namespace UserApi.Features.Users.Create;

public static class CreateUserEndpoint
{
    public static IEndpointRouteBuilder MapCreateUserEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/users", async (
            [FromBody] CreateUserPayload payload, 
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new CreateUserCommand(payload), cancellationToken);
            return result.ToTypedResults(() => TypedResults.Created());
        })
        .WithName("CreateUser")
        .WithTags("Users")
        .WithDescription("Creates a new user with the specified information.")
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status406NotAcceptable)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        return app;
    }
}