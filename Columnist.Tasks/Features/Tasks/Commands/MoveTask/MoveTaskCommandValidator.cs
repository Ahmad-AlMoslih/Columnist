using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Tasks.Features.Tasks.Commands.MoveTask;

public sealed class MoveTaskCommandValidator : AbstractValidator<MoveTaskCommand>
{
    public MoveTaskCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(ErrorMessages.TaskNotFound);
        RuleFor(x => x.ColumnToken)
            .NotEmpty().WithMessage(ErrorMessages.ChooseStatus);
    }
}
