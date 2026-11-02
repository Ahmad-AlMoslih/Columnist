using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Dms;
using Columnist.Services.Tokens.Extensions;
using Columnist.Tasks.Features.Tasks.Queries.GetBoard.Models;
using Columnist.Utilities.Constants;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using MediatR;

namespace Columnist.Tasks.Features.Tasks.Queries.GetBoard;

public sealed class GetBoardHandler : IRequestHandler<GetBoardQuery, ApiResponse<GetBoardModel>>
{
    private readonly IDbExecutor _db;
    private readonly IDmsService _dms;

    public GetBoardHandler(IDbExecutor db, IDmsService dms)
    {
        _db = db;
        _dms = dms;
    }

    public async Task<ApiResponse<GetBoardModel>> Handle(GetBoardQuery request, CancellationToken cancellationToken)
    {
        var columns = await GetBoardColumnsAsync(cancellationToken);

        var boards = columns
            .DistinctBy(c => c.BoardId)
            .Select(c => new OptionDto { Id = c.BoardId, Name = c.BoardName })
            .ToList();

        OptionDto? selected;
        if (string.IsNullOrEmpty(request.BoardToken))
        {
            selected = boards.FirstOrDefault();
        }
        else
        {
            var boardId = request.BoardToken.Decrypt<int>();
            if (boardId is null)
                return ApiResponse<GetBoardModel>.NotFound(ErrorMessages.BoardNotFound);

            selected = boards.FirstOrDefault(b => b.Id == boardId.Value);
            if (selected is null)
                return ApiResponse<GetBoardModel>.NotFound(ErrorMessages.BoardNotFound);
        }

        if (selected is null)
            return ApiResponse<GetBoardModel>.Success(new GetBoardModel());

        var tabs = boards
            .Select(b => new TokenOptionDto { Token = b.Id.Encrypt(), Name = b.Name, Selected = b.Id == selected.Id })
            .ToList();

        var tasks = (await GetAllTasksAsync(cancellationToken))
            .Where(t => t.BoardId == selected.Id)
            .ToList();
        var search = request.Search?.Trim();
        if (search is { Length: >= SearchRules.MinLength })
            tasks = tasks.Where(t => t.Title.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

        return ApiResponse<GetBoardModel>.Success(new GetBoardModel
        {
            Boards = tabs,
            SelectedBoardToken = tabs.First(t => t.Selected).Token,
            SelectedBoardName = selected.Name,
            Search = request.Search,
            Columns = columns
                .Where(c => c.BoardId == selected.Id)
                .OrderBy(c => c.Position)
                .Select(c => new GetBoardColumnModel
                {
                    ColumnToken = c.ColumnId.Encrypt(),
                    Name = c.ColumnName,
                    StatusColor = c.StatusColor,
                    Tasks = tasks.Where(t => t.ColumnId == c.ColumnId).Select(ToTaskModel).ToList()
                })
                .ToList()
        });
    }

    private GetBoardTaskModel ToTaskModel(TaskItemDto task)
    {
        return new GetBoardTaskModel
        {
            Token = task.Id.Encrypt(),
            Title = task.Title,
            Description = task.Description,
            ColumnName = task.ColumnName,
            AssigneeName = task.AssigneeName,
            DueDate = task.DueDate,
            ImageCount = task.ImageCount,
            FirstImageUrl = _dms.GetUrl(DmsFolder.Task, task.FirstImageName)
        };
    }

    private async Task<IReadOnlyList<BoardColumnDto>> GetBoardColumnsAsync(CancellationToken ct)
    {
        return await _db.QueryAsync<BoardColumnDto>("dbo.GetAllBoardsWithColumns", ct: ct);
    }

    private async Task<IReadOnlyList<TaskItemDto>> GetAllTasksAsync(CancellationToken ct)
    {
        return await _db.QueryAsync<TaskItemDto>("dbo.GetAllTasksWithDetails", ct: ct);
    }
}
