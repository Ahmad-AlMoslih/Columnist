using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Tasks.Features.Reports.Queries.GetBoardReport;

public sealed class GetBoardReportQueryValidator : AbstractValidator<GetBoardReportQuery>
{
    public GetBoardReportQueryValidator()
    {
        RuleFor(x => x.BoardToken)
            .NotEmpty().WithMessage(ErrorMessages.BoardNotFound);

        RuleFor(x => x.Filter!.Q)
            .MaximumLength(100).WithMessage(ErrorMessages.ValueTooLong)
            .When(x => x.Filter is not null);
    }
}
