using Columnist.Contracts.Dtos;
using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Tasks.Features.Tasks.Commands.SetTaskAssignees;

public sealed class SetTaskAssigneesCommandValidator : AbstractValidator<SetTaskAssigneesCommand>
{
    public SetTaskAssigneesCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(ErrorMessages.TaskNotFound);

        RuleFor(x => x.AssigneeTokens)
            .Must(tokens => tokens is null || tokens.Count <= TaskFormDto.MaxAssignees)
            .WithMessage(string.Format(ErrorMessages.TooManyAssignees, TaskFormDto.MaxAssignees));
    }
}
