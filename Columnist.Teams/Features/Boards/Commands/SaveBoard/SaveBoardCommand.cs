using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Teams.Features.Boards.Commands.SaveBoard;

// BoardToken empty = new board in TeamToken's team. Columns are in display order; a column without Token is new; a column left out is deleted.
public sealed record SaveBoardCommand(string? BoardToken, string? TeamToken, string Name, List<SaveBoardColumn>? Columns) : IRequest<ApiResponse<string>>
{
    public const int NameMaxLength = 100;        // = Boards.Name NVARCHAR(100)
    public const int ColumnNameMaxLength = 50;   // = BoardColumns.Name NVARCHAR(50)
}

public sealed record SaveBoardColumn(string? Token, string Name, string StatusName);
