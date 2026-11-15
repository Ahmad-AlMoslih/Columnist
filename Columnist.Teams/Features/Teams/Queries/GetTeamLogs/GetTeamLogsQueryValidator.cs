using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Teams.Features.Teams.Queries.GetTeamLogs;

public sealed class GetTeamLogsQueryValidator : AbstractValidator<GetTeamLogsQuery>
{
    public GetTeamLogsQueryValidator()
    {
        RuleFor(x => x.TeamToken)
            .NotEmpty().WithMessage(ErrorMessages.TeamNotFound);

        RuleFor(x => x.Category)
            .Must(c => string.IsNullOrEmpty(c) || c is "Task" or "Board" or "Access" or "Team").WithMessage(ErrorMessages.InvalidData);

        RuleFor(x => x.Search)
            .MaximumLength(100).WithMessage(ErrorMessages.ValueTooLong);

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage(ErrorMessages.InvalidData);
    }
}
