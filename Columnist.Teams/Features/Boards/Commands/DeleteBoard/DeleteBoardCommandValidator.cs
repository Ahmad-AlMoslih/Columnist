using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Teams.Features.Boards.Commands.DeleteBoard;

public sealed class DeleteBoardCommandValidator : AbstractValidator<DeleteBoardCommand>
{
    public DeleteBoardCommandValidator()
    {
        RuleFor(x => x.BoardToken)
            .NotEmpty().WithMessage(ErrorMessages.BoardNotFound);
    }
}
