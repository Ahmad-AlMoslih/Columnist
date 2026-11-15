using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Tasks.Features.Tasks.Commands.SaveComment;

public sealed class SaveCommentCommandValidator : AbstractValidator<SaveCommentCommand>
{
    public SaveCommentCommandValidator()
    {
        RuleFor(x => x.TaskToken)
            .NotEmpty().WithMessage(ErrorMessages.TaskNotFound);

        RuleFor(x => x.Body)
            .Must(body => !string.IsNullOrWhiteSpace(body)).WithMessage(ErrorMessages.CommentRequired)
            .MaximumLength(SaveCommentCommand.BodyMaxLength).WithMessage(ErrorMessages.CommentTooLong);
    }
}
