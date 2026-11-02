using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Dms;
using Columnist.Services.Tokens.Extensions;
using Columnist.Tasks.Features.Tasks.Queries.GetDeletedTasks.Models;
using Columnist.Utilities.Constants;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Tasks.Features.Tasks.Queries.GetDeletedTasks;

public sealed class GetDeletedTasksHandler : IRequestHandler<GetDeletedTasksQuery, ApiResponse<GetDeletedTasksModel>>
{
    private readonly IDbExecutor _db;
    private readonly IDmsService _dms;

    public GetDeletedTasksHandler(IDbExecutor db, IDmsService dms)
    {
        _db = db;
        _dms = dms;
    }

    public async Task<ApiResponse<GetDeletedTasksModel>> Handle(GetDeletedTasksQuery request, CancellationToken cancellationToken)
    {
        var rows = await _db.QueryAsync<DeletedTaskDto>("dbo.GetDeletedTasks", ct: cancellationToken);
        var search = request.Search?.Trim();
        if (search is { Length: >= SearchRules.MinLength })
            rows = rows.Where(r => r.Title.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        var images = (await _db.QueryAsync<DeletedTaskImageDto>("dbo.GetDeletedTaskImages", ct: cancellationToken))
            .ToLookup(i => i.TaskId);

        IReadOnlyList<DeletedTaskModel> tasks = rows
            .Select(t => new DeletedTaskModel
            {
                Token = t.Id.Encrypt(),
                Title = t.Title,
                BoardName = t.BoardName,
                ColumnName = t.ColumnName,
                DeletedAt = t.DeletedAt,
                DeletedByName = t.DeletedByName,
                ImageUrls = images[t.Id].Select(i => _dms.GetUrl(DmsFolder.Task, i.FileName)!).ToList()
            })
            .ToList();

        return ApiResponse<GetDeletedTasksModel>.Success(new GetDeletedTasksModel { Tasks = tasks, Search = request.Search });
    }
}
