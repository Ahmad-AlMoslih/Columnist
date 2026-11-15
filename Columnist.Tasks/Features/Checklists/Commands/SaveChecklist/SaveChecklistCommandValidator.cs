using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Tasks.Features.Checklists.Commands.SaveChecklist;

public sealed class SaveChecklistCommandValidator : AbstractValidator<SaveChecklistCommand>
{
    public SaveChecklistCommandValidator()
    {
        RuleFor(x => x.TaskToken)
            .NotEmpty().WithMessage(ErrorMessages.ChecklistNotFound);

        RuleFor(x => (x.Title ?? "").Trim())
            .NotEmpty().WithMessage(ErrorMessages.ChecklistTitleRequired)
            .MaximumLength(SaveChecklistCommand.TitleMaxLength).WithMessage(ErrorMessages.ChecklistTitleTooLong)
            .OverridePropertyName(nameof(SaveChecklistCommand.Title));
    }
}
