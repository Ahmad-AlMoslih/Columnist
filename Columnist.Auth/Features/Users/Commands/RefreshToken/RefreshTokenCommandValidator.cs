using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Auth.Features.Users.Commands.RefreshToken;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage(ErrorMessages.Unauthorized);
    }
}
