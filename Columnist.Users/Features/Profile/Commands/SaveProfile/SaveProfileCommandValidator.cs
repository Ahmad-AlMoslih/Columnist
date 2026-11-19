using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Users.Features.Profile.Commands.SaveProfile;

public sealed class SaveProfileCommandValidator : AbstractValidator<SaveProfileCommand>
{
    public SaveProfileCommandValidator()
    {
        // Both are checked trimmed, as they are saved.
        RuleFor(x => (x.FullName ?? "").Trim())
            .NotEmpty().WithMessage(ErrorMessages.FullNameRequired)
            .MaximumLength(SaveProfileCommand.FullNameMaxLength).WithMessage(ErrorMessages.FullNameTooLong)
            .OverridePropertyName(nameof(SaveProfileCommand.FullName));

        RuleFor(x => (x.Username ?? "").Trim())
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ErrorMessages.UsernameRequired)
            .Length(SaveProfileCommand.UsernameMinLength, SaveProfileCommand.UsernameMaxLength).WithMessage(ErrorMessages.UsernameLength)
            .Matches("^[A-Za-z0-9._-]+$").WithMessage(ErrorMessages.UsernameInvalidCharacters)
            .OverridePropertyName(nameof(SaveProfileCommand.Username));
    }
}
