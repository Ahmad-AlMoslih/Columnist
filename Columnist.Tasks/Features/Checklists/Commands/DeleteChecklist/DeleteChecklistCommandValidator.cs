using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Tasks.Features.Checklists.Commands.DeleteChecklist;

public sealed class DeleteChecklistCommandValidator : AbstractValidator<DeleteChecklistCommand>
{
    public DeleteChecklistCommandValidator()
    {
        RuleFor(x => x.TaskToken)
            .NotEmpty().WithMessage(ErrorMessages.ChecklistNotFound);

        RuleFor(x => x.ChecklistToken)
            .NotEmpty().WithMessage(ErrorMessages.ChecklistNotFound);
    }
}
