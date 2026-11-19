using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Tasks.Features.Tasks.Commands.UpdateTaskDueDate;

// "Can't be in the past" needs the current value, so the handler checks it.
public sealed class UpdateTaskDueDateCommandValidator : AbstractValidator<UpdateTaskDueDateCommand>
{
    public UpdateTaskDueDateCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(ErrorMessages.TaskNotFound);
    }
}
