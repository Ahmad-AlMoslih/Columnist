using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Users.Features.Users.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ErrorMessages.EmailRequired);
    }
}
