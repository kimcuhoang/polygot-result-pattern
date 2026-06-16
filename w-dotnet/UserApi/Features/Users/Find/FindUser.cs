using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UserApi.Infrastructures.EntityFramework;
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

internal class FindUserHandler(UserDbContext dbContext) : IRequestHandler<FindUserRequest, Result<FindUserResult>>
{
    public async Task<Result<FindUserResult>> Handle(FindUserRequest request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Payload.Email, cancellationToken);
        if (user == null)
        {
            return Result<FindUserResult>.Failure(Reason.NotFound("User not found."));
        }

        return Result<FindUserResult>.Success(new FindUserResult(user.Id, user.Name, user.Email));
    }
}