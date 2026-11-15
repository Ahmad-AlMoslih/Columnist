using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Teams.Features.Teams.Commands.JoinTeam;

public sealed class JoinTeamCommandValidator : AbstractValidator<JoinTeamCommand>
{
    public JoinTeamCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage(ErrorMessages.InviteCodeRequired);
    }
}
