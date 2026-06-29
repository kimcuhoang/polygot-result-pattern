using FluentValidation;
using MediatR;
using UserApi.Infrastructures.EntityFramework;
using UserApi.Models;
using Microsoft.EntityFrameworkCore;
using UserApi.Domains;
using EntityFramework.Exceptions.Common;

namespace UserApi.Features.Users.Create;

public record CreateUserCommand(CreateUserPayload Payload) : IRequest<Result> { }

internal class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
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

        return payload.Email switch
        {
            "exception@email.com" => throw new Exception("An unexpected error occurred."),
            "not-accept@email.com" => Result.Failure(Reason.NotAcceptable("This email address is not acceptable.")),
            _ => await HandleEmailCheckAsync(payload, cancellationToken)
        };
    }

    private async Task<Result> HandleEmailCheckAsync(CreateUserPayload payload, CancellationToken cancellationToken)
    {
        try
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = payload.Name,
                Email = payload.Email
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintException ex)
        {
            return Result.Failure(Reason.Conflict($"A user with {string.Join(", ", ex.ConstraintProperties)} already exists."));
        }

        return Result.Success();
    }
}