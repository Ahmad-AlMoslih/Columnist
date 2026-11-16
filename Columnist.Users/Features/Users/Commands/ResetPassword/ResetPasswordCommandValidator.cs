using Columnist.Users.Features.Profile.Commands.ChangePassword;
using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Users.Features.Users.Commands.ResetPassword;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.NewPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ErrorMessages.NewPasswordRequired)
            .MinimumLength(ChangePasswordCommand.PasswordMinLength).WithMessage(ErrorMessages.NewPasswordTooShort)
            .MaximumLength(ChangePasswordCommand.PasswordMaxLength).WithMessage(ErrorMessages.NewPasswordTooLong);

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword).WithMessage(ErrorMessages.PasswordsDontMatch);
    }
}
