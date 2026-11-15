using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Users.Features.Profile.Commands.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage(ErrorMessages.CurrentPasswordRequired);

        RuleFor(x => x.NewPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ErrorMessages.NewPasswordRequired)
            .MinimumLength(ChangePasswordCommand.PasswordMinLength).WithMessage(ErrorMessages.NewPasswordTooShort)
            .MaximumLength(ChangePasswordCommand.PasswordMaxLength).WithMessage(ErrorMessages.NewPasswordTooLong);

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword).WithMessage(ErrorMessages.PasswordsDontMatch);
    }
}
