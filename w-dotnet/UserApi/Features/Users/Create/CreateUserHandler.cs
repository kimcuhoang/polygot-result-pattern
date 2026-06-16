using FluentValidation;
using MediatR;
using UserApi.Infrastructures.EntityFramework;
using UserApi.Models;
using Microsoft.EntityFrameworkCore;

namespace UserApi.Features.Users.Create;

public record CreateUserCommand(CreateUserPayload Payload) : IRequest<Result> {}

internal class CreateUserCommandValidator: AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(IValidator<CreateUserPayload> payloadValidator)
    {
        RuleFor(x => x.Payload)
        .NotNull()
        .WithMessage("Payload is required.")
        .OverridePropertyName("")
        .SetValidator(payloadValidator);
    }
}

internal class CreateUserHandler(UserDbContext dbContext) : IRequestHandler<CreateUserCommand, Result>
{
    public async Task<Result> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var payload = request.Payload;

        if (payload.Email == "not-accept@email.com")
        {
            return Result.Failure(Reason.NotAcceptable("This email address is not acceptable."));
        }
        
        var emailHasBeenTaken = await dbContext.Users.AnyAsync(u => u.Email == payload.Email, cancellationToken);
        if (emailHasBeenTaken)
        {
            return Result.Failure(Reason.Conflict("This email address is already in use."));
        }

        if (payload.Email == "exception@email.com")
        {
            throw new Exception("An unexpected error occurred.");
        }

        return Result.Success();
    }
}