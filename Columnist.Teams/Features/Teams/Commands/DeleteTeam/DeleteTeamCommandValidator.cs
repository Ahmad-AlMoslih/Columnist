using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Teams.Features.Teams.Commands.DeleteTeam;

public sealed class DeleteTeamCommandValidator : AbstractValidator<DeleteTeamCommand>
{
    public DeleteTeamCommandValidator()
    {
        RuleFor(x => x.TeamToken)
            .NotEmpty().WithMessage(ErrorMessages.TeamNotFound);
    }
}
