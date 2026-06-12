using FluentValidation;

namespace UserApi.Features.Users.Create;
public record CreateUserPayload(
    string Email
);

internal class CreateUserPayloadValidator: AbstractValidator<CreateUserPayload>
{
    public CreateUserPayloadValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.");
    }
}

