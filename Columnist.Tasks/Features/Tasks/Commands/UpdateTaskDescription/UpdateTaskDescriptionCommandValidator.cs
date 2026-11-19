using Columnist.Contracts.Dtos;
using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Tasks.Features.Tasks.Commands.UpdateTaskDescription;

public sealed class UpdateTaskDescriptionCommandValidator : AbstractValidator<UpdateTaskDescriptionCommand>
{
    public UpdateTaskDescriptionCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(ErrorMessages.TaskNotFound);

        RuleFor(x => x.Description)
            .MaximumLength(TaskFormDto.DescriptionMaxLength)
            .WithMessage(string.Format(ErrorMessages.DescriptionTooLong, nameof(TaskFormDto.Description), TaskFormDto.DescriptionMaxLength));
    }
}
