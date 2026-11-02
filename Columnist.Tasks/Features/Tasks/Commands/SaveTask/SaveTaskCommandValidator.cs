using Columnist.Contracts.Dtos;
using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Tasks.Features.Tasks.Commands.SaveTask;

public sealed class SaveTaskCommandValidator : AbstractValidator<SaveTaskCommand>
{
    public SaveTaskCommandValidator()
    {
        RuleFor(x => x.Task.Title)
            .NotEmpty().WithMessage(ErrorMessages.TitleRequired)
            .MaximumLength(TaskFormDto.TitleMaxLength)
            .WithMessage(string.Format(ErrorMessages.TitleTooLong, nameof(TaskFormDto.Title), TaskFormDto.TitleMaxLength))
            .OverridePropertyName(nameof(TaskFormDto.Title));

        RuleFor(x => x.Task.Description)
            .MaximumLength(TaskFormDto.DescriptionMaxLength)
            .WithMessage(string.Format(ErrorMessages.DescriptionTooLong, nameof(TaskFormDto.Description), TaskFormDto.DescriptionMaxLength))
            .OverridePropertyName(nameof(TaskFormDto.Description));

        RuleFor(x => x.Task.ColumnToken)
            .NotEmpty().WithMessage(ErrorMessages.ChooseStatus)
            .OverridePropertyName(nameof(TaskFormDto.ColumnToken));
    }
}
