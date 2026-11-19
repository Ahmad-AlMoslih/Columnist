using Columnist.Auth.Security.Boards;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Dms;
using Columnist.Services.Tokens.Extensions;
using Columnist.Tasks.Features.Tasks.Queries.GetTaskView.Models;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;

namespace Columnist.Tasks.Features.Tasks.Queries.GetNewTaskView;

public sealed class GetNewTaskViewHandler : IRequestHandler<GetNewTaskViewQuery, ApiResponse<GetTaskViewModel>>
{
    private readonly IDbExecutor _db;
    private readonly IBoardAccessService _boards;
    private readonly DmsOptions _dmsOptions;

    public GetNewTaskViewHandler(IDbExecutor db, IBoardAccessService boards, DmsOptions dmsOptions)
    {
        _db = db;
        _boards = boards;
        _dmsOptions = dmsOptions;
    }

    public async Task<ApiResponse<GetTaskViewModel>> Handle(GetNewTaskViewQuery request, CancellationToken cancellationToken)
    {
        // The board, resolved once (cached since the pipeline's check, which also checked TasksWrite on it).
        var boardIds = await _boards.ResolveAsync(request.GetBoardScope(), cancellationToken);
        if (boardIds is not [var boardId])
            return ApiResponse<GetTaskViewModel>.NotFound(ErrorMessages.BoardNotFound);

        var columns = await GetColumnOptionsAsync(boardId, cancellationToken);
        if (columns.Count == 0)
            return ApiResponse<GetTaskViewModel>.NotFound(ErrorMessages.BoardNotFound);

        var requestedId = request.ColumnToken.Decrypt<int>();
        var selected = columns.FirstOrDefault(c => c.ColumnId == requestedId) ?? columns[0];

        return ApiResponse<GetTaskViewModel>.Success(new GetTaskViewModel
        {
            IsDraft = true,
            CanWrite = true,
            BoardName = selected.BoardName,
            ColumnName = selected.ColumnName,
            StatusName = selected.StatusName,
            StatusColor = selected.StatusColor,
            BoardToken = boardId.Encrypt(),
            ColumnToken = selected.ColumnId.Encrypt(),
            Columns = columns
                .Select(c => new TaskColumnOptionModel(c.ColumnId.Encrypt(), c.ColumnName, c.StatusName, c.StatusColor, c.ColumnId == selected.ColumnId))
                .ToList(),
            // The draft shows the (inactive) image tile: same limits as the task view.
            UploadMaxFiles = _dmsOptions.MaxFilesPerUpload,
            UploadMaxBytes = _dmsOptions.MaxFileSizeBytes,
            UploadAllowedTypes = _dmsOptions.AllowedExtensions
        });
    }

    private async Task<IReadOnlyList<ColumnOptionDto>> GetColumnOptionsAsync(int boardId, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@BoardId", boardId);
        return await _db.QueryAsync<ColumnOptionDto>("dbo.GetBoardColumnOptions", parameters, ct);
    }
}
