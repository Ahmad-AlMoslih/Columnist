using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Teams.Features.Teams.Commands.SaveTeam;

public sealed class SaveTeamCommandValidator : AbstractValidator<SaveTeamCommand>
{
    public SaveTeamCommandValidator()
    {
        RuleFor(x => x.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage(ErrorMessages.TeamNameRequired)
            .MaximumLength(SaveTeamCommand.NameMaxLength).WithMessage(ErrorMessages.TeamNameTooLong);
    }
}
