using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Tasks.Features.Tasks.Queries.GetBoardCard;

public sealed class GetBoardCardQueryValidator : AbstractValidator<GetBoardCardQuery>
{
    public GetBoardCardQueryValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(ErrorMessages.TaskNotFound);
    }
}
