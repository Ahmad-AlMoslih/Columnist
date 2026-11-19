using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Tasks.Features.Tasks.Commands.RemoveTaskImage;

public sealed class RemoveTaskImageCommandValidator : AbstractValidator<RemoveTaskImageCommand>
{
    public RemoveTaskImageCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(ErrorMessages.TaskNotFound);

        RuleFor(x => x.ImageToken)
            .NotEmpty().WithMessage(ErrorMessages.ImageNotFound);
    }
}
