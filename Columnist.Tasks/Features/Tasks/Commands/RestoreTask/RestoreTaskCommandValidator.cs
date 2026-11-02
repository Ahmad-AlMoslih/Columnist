using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Tasks.Features.Tasks.Commands.RestoreTask;

public sealed class RestoreTaskCommandValidator : AbstractValidator<RestoreTaskCommand>
{
    public RestoreTaskCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(ErrorMessages.TaskNotFound);
    }
}
