using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Teams.Features.BoardAccess.Commands.SetBoardMemberPermission;

public sealed class SetBoardMemberPermissionCommandValidator : AbstractValidator<SetBoardMemberPermissionCommand>
{
    public SetBoardMemberPermissionCommandValidator()
    {
        RuleFor(x => x.BoardToken)
            .NotEmpty().WithMessage(ErrorMessages.BoardNotFound);

        RuleFor(x => x.MemberToken).NotEmpty().WithMessage(ErrorMessages.PermissionChangeFailed);

        RuleFor(x => x.PermissionName).NotEmpty().WithMessage(ErrorMessages.PermissionChangeFailed);
    }
}
