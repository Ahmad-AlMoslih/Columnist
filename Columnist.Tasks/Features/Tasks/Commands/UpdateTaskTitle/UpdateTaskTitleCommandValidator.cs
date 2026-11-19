using Columnist.Contracts.Dtos;
using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Tasks.Features.Tasks.Commands.UpdateTaskTitle;

public sealed class UpdateTaskTitleCommandValidator : AbstractValidator<UpdateTaskTitleCommand>
{
    public UpdateTaskTitleCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(ErrorMessages.TaskNotFound);

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(ErrorMessages.TitleRequired)
            .MaximumLength(TaskFormDto.TitleMaxLength)
            .WithMessage(string.Format(ErrorMessages.TitleTooLong, nameof(TaskFormDto.Title), TaskFormDto.TitleMaxLength));
    }
}
