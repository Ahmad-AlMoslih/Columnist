using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Tasks.Features.Reports.Queries.GetDashboardCard;

public sealed class GetDashboardCardQueryValidator : AbstractValidator<GetDashboardCardQuery>
{
    public GetDashboardCardQueryValidator()
    {
        RuleFor(x => x.BoardToken)
            .NotEmpty().WithMessage(ErrorMessages.BoardNotFound);
    }
}
