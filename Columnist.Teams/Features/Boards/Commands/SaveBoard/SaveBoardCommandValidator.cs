using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Teams.Features.Boards.Commands.SaveBoard;

public sealed class SaveBoardCommandValidator : AbstractValidator<SaveBoardCommand>
{
    public SaveBoardCommandValidator()
    {
        RuleFor(x => x.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage(ErrorMessages.BoardNameRequired)
            .MaximumLength(SaveBoardCommand.NameMaxLength).WithMessage(ErrorMessages.BoardNameTooLong);

        RuleFor(x => x.Columns)
            .NotEmpty().WithMessage(ErrorMessages.BoardNeedsColumn);

        RuleForEach(x => x.Columns).ChildRules(column =>
        {
            column.RuleFor(c => c.Name)
                .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage(ErrorMessages.ColumnNameRequired)
                .MaximumLength(SaveBoardCommand.ColumnNameMaxLength).WithMessage(ErrorMessages.ColumnNameTooLong);

            column.RuleFor(c => c.StatusName)
                .NotEmpty().WithMessage(ErrorMessages.StatusNotFound);
        });
    }
}