using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Teams.Features.Teams.Commands.RemoveTeamMember;

public sealed class RemoveTeamMemberCommandValidator : AbstractValidator<RemoveTeamMemberCommand>
{
    public RemoveTeamMemberCommandValidator()
    {
        RuleFor(x => x.TeamToken)
            .NotEmpty().WithMessage(ErrorMessages.TeamNotFound);

        RuleFor(x => x.MemberToken).NotEmpty().WithMessage(ErrorMessages.TeamMemberNotFound);
    }
}
