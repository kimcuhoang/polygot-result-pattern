using System.ComponentModel.DataAnnotations;
using MediatR;
using UserApi.Models;

namespace UserApi.Features.Users.Find;

public record FindUserPayload(
    // This cause the exception of reflection
    // "property:" save the day
    [property: Required, EmailAddress]
    string Email
);

public record FindUserResult(
    Guid Id,
    string Name,
    string Email
);

public record FindUserRequest(FindUserPayload Payload) : IRequest<Result<FindUserResult>>;

internal class FindUserHandler : IRequestHandler<FindUserRequest, Result<FindUserResult>>
{
    public Task<Result<FindUserResult>> Handle(FindUserRequest request, CancellationToken cancellationToken)
    {
        // Simulate finding a user by email
        if (request.Payload.Email == "notfound@email.com")
        {
            return Task.FromResult(Result<FindUserResult>.Failure(Reason.NotFound("User not found.")));
        }

        return Task.FromResult(Result<FindUserResult>.Success(new FindUserResult(
            Id: Guid.NewGuid(),
            Name: "John Doe",
            Email: request.Payload.Email
        )));
    }
}