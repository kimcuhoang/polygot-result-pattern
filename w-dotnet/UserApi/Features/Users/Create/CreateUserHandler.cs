using FluentValidation;
using MediatR;
using UserApi.Models;

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

internal class CreateUserHandler : IRequestHandler<CreateUserCommand, Result>
{
    public async Task<Result> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var payload = request.Payload;

        if (payload.Email == "not-accept@email.com")
        {
            return Result.Failure(Reason.NotAcceptable("This email address is not acceptable."));
        }

        if (payload.Email == "duplicate@email.com")
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