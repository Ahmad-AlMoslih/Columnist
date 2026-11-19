using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Tasks.Features.Tasks.Queries.GetBoardFilterMembers;

public sealed class GetBoardFilterMembersQueryValidator : AbstractValidator<GetBoardFilterMembersQuery>
{
    public GetBoardFilterMembersQueryValidator()
    {
        RuleFor(x => x.BoardToken)
            .NotEmpty().WithMessage(ErrorMessages.BoardNotFound);
    }
}
