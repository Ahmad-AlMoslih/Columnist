using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Users.Features.Users.Commands.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ErrorMessages.EmailRequired);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ErrorMessages.PasswordRequired);
    }
}
