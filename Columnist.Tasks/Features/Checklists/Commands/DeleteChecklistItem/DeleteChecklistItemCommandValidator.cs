using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Tasks.Features.Checklists.Commands.DeleteChecklistItem;

public sealed class DeleteChecklistItemCommandValidator : AbstractValidator<DeleteChecklistItemCommand>
{
    public DeleteChecklistItemCommandValidator()
    {
        RuleFor(x => x.TaskToken)
            .NotEmpty().WithMessage(ErrorMessages.ChecklistNotFound);

        RuleFor(x => x.ChecklistToken)
            .NotEmpty().WithMessage(ErrorMessages.ChecklistNotFound);

        RuleFor(x => x.ItemToken)
            .NotEmpty().WithMessage(ErrorMessages.ChecklistNotFound);
    }
}
