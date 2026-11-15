using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Users.Features.Profile.Commands.SaveFullName;

public sealed class SaveFullNameCommandValidator : AbstractValidator<SaveFullNameCommand>
{
    public SaveFullNameCommandValidator()
    {
        // Checked on the trimmed name, as it is saved.
        RuleFor(x => (x.FullName ?? "").Trim())
            .NotEmpty().WithMessage(ErrorMessages.FullNameRequired)
            .MaximumLength(SaveFullNameCommand.FullNameMaxLength).WithMessage(ErrorMessages.FullNameTooLong)
            .OverridePropertyName(nameof(SaveFullNameCommand.FullName));
    }
}
