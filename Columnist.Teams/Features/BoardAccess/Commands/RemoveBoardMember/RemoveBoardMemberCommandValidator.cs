using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Teams.Features.BoardAccess.Commands.RemoveBoardMember;

public sealed class RemoveBoardMemberCommandValidator : AbstractValidator<RemoveBoardMemberCommand>
{
    public RemoveBoardMemberCommandValidator()
    {
        RuleFor(x => x.BoardToken)
            .NotEmpty().WithMessage(ErrorMessages.BoardNotFound);

        RuleFor(x => x.MemberToken).NotEmpty().WithMessage(ErrorMessages.PermissionChangeFailed);
    }
}
