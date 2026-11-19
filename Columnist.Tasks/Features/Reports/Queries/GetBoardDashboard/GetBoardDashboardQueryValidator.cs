using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Tasks.Features.Reports.Queries.GetBoardDashboard;

public sealed class GetBoardDashboardQueryValidator : AbstractValidator<GetBoardDashboardQuery>
{
    public GetBoardDashboardQueryValidator()
    {
        RuleFor(x => x.BoardToken)
            .NotEmpty().WithMessage(ErrorMessages.BoardNotFound);
    }
}
