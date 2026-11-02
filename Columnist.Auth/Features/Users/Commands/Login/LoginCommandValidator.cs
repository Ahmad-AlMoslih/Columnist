using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Auth.Features.Users.Commands.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(ErrorMessages.UsernameRequired);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ErrorMessages.PasswordRequired);
    }
}
