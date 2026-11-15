using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Tasks.Features.Checklists.Commands.SaveChecklistItem;

public sealed class SaveChecklistItemCommandValidator : AbstractValidator<SaveChecklistItemCommand>
{
    public SaveChecklistItemCommandValidator()
    {
        RuleFor(x => x.TaskToken)
            .NotEmpty().WithMessage(ErrorMessages.ChecklistNotFound);

        RuleFor(x => x.ChecklistToken)
            .NotEmpty().WithMessage(ErrorMessages.ChecklistNotFound);

        RuleFor(x => (x.Text ?? "").Trim())
            .NotEmpty().WithMessage(ErrorMessages.ChecklistItemRequired)
            .MaximumLength(SaveChecklistItemCommand.TextMaxLength).WithMessage(ErrorMessages.ChecklistItemTooLong)
            .OverridePropertyName(nameof(SaveChecklistItemCommand.Text));
    }
}
