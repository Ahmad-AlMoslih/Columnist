using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Teams.Features.BoardAccess.Commands.AddBoardMember;

public sealed class AddBoardMemberCommandValidator : AbstractValidator<AddBoardMemberCommand>
{
    public AddBoardMemberCommandValidator()
    {
        RuleFor(x => x.BoardToken)
            .NotEmpty().WithMessage(ErrorMessages.BoardNotFound);

        RuleFor(x => x.MemberToken).NotEmpty().WithMessage(ErrorMessages.PermissionChangeFailed);
    }
}
