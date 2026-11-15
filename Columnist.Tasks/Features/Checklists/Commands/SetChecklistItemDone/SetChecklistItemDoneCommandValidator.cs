using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Tasks.Features.Checklists.Commands.SetChecklistItemDone;

public sealed class SetChecklistItemDoneCommandValidator : AbstractValidator<SetChecklistItemDoneCommand>
{
    public SetChecklistItemDoneCommandValidator()
    {
        RuleFor(x => x.TaskToken)
            .NotEmpty().WithMessage(ErrorMessages.ChecklistNotFound);

        RuleFor(x => x.ChecklistToken)
            .NotEmpty().WithMessage(ErrorMessages.ChecklistNotFound);

        RuleFor(x => x.ItemToken)
            .NotEmpty().WithMessage(ErrorMessages.ChecklistNotFound);
    }
}
