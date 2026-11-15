using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Tasks.Features.Logs.Queries.GetBoardLogs;

public sealed class GetBoardLogsQueryValidator : AbstractValidator<GetBoardLogsQuery>
{
    public GetBoardLogsQueryValidator()
    {
        RuleFor(x => x.BoardToken)
            .NotEmpty().WithMessage(ErrorMessages.BoardNotFound);

        RuleFor(x => x.Category)
            .Must(c => string.IsNullOrEmpty(c) || c is "Task" or "Board" or "Access").WithMessage(ErrorMessages.InvalidData);

        RuleFor(x => x.Search)
            .MaximumLength(100).WithMessage(ErrorMessages.ValueTooLong);

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage(ErrorMessages.InvalidData);
    }
}
