using Columnist.Auth.Security.CurrentUser;
using Columnist.Auth.Security.Photos;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Dms;
using Columnist.Services.Tokens.Extensions;
using Columnist.Tasks.Features.Tasks.Models;
using Columnist.Tasks.Features.Tasks.Queries.GetBoard.Models;
using Columnist.Utilities.Constants;
using Columnist.Utilities.Response;
using Columnist.Utilities.Response.Errors;
using Dapper;
using MediatR;
using System.Globalization;

namespace Columnist.Tasks.Features.Tasks.Queries.GetBoard;

public sealed class GetBoardHandler : IRequestHandler<GetBoardQuery, ApiResponse<GetBoardModel>>
{
    private readonly IDbExecutor _db;
    private readonly IDmsService _dms;
    private readonly IUserContext _userContext;
    private readonly IUserPhotoService _photos;

    public GetBoardHandler(IDbExecutor db, IDmsService dms, IUserContext userContext, IUserPhotoService photos)
    {
        _db = db;
        _dms = dms;
        _userContext = userContext;
        _photos = photos;
    }

    public async Task<ApiResponse<GetBoardModel>> Handle(GetBoardQuery request, CancellationToken cancellationToken)
    {
        var teamId = request.TeamToken?.Decrypt<int>();
        if (teamId is null)
            return ApiResponse<GetBoardModel>.Success(new GetBoardModel());

        var columns = await GetBoardColumnsAsync(teamId.Value, cancellationToken);

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

        var allTasks = await GetAllTasksAsync(selected.Id, cancellationToken);

        var assigneeRows = await GetTaskAssigneesAsync(null, selected.Id, cancellationToken);
        var photos = await _photos.GetUrlsAsync(cancellationToken);
        var assignees = assigneeRows.ToLookup(a => a.TaskId, a => TaskAssigneeModel.From(a) with { PhotoUrl = photos.GetValueOrDefault(a.UserId) });
        var assigneeIdsByTask = assigneeRows.ToLookup(a => a.TaskId, a => a.UserId);

        var checklistCounts = (await GetBoardChecklistCountsAsync(selected.Id, cancellationToken))
            .ToDictionary(c => c.TaskId);

        var users = await GetUserOptionsAsync(selected.Id, cancellationToken);
        var filter = BuildFilter(request.Filter ?? new BoardFilterRequest(), users, photos, out var chosenIds);

        IEnumerable<TaskItemDto> shown = allTasks;
        var search = request.Search?.Trim();
        if (search is { Length: >= SearchRules.MinLength })
            shown = shown.Where(t => t.Title.Contains(search, StringComparison.OrdinalIgnoreCase));
        var tasks = shown
            .Where(t => Matches(t, filter, chosenIds, assigneeIdsByTask[t.Id], checklistCounts.GetValueOrDefault(t.Id)))
            .ToList();

        return ApiResponse<GetBoardModel>.Success(new GetBoardModel
        {
            Boards = tabs,
            SelectedBoardToken = tabs.First(t => t.Selected).Token,
            SelectedBoardName = selected.Name,
            Search = request.Search,
            Filter = filter,
            Columns = columns
                .Where(c => c.BoardId == selected.Id)
                .OrderBy(c => c.Position)
                .Select(c => new GetBoardColumnModel
                {
                    ColumnToken = c.ColumnId.Encrypt(),
                    Name = c.ColumnName,
                    StatusColor = c.StatusColor,
                    Tasks = tasks.Where(t => t.ColumnId == c.ColumnId).Select(t => ToTaskModel(t, assignees[t.Id].ToList(), checklistCounts.GetValueOrDefault(t.Id))).ToList(),
                    TotalTaskCount = allTasks.Count(t => t.ColumnId == c.ColumnId)
                })
                .ToList()
        });
    }

    // Reads the filter's query values, ignoring invalid ones. chosenIds = the chosen people ("Me" and board members).
    private BoardFilterModel BuildFilter(BoardFilterRequest request, IReadOnlyList<UserOptionDto> users, IReadOnlyDictionary<int, string> photos, out HashSet<int> chosenIds)
    {
        var meId = _userContext.UserId ?? 0;

        // Keep the token each person was sent with, so the open filter panel and the chips match.
        var sentTokens = new Dictionary<int, string>();
        foreach (var token in request.Assignees ?? [])
        {
            var id = token.Decrypt<int>();
            if (id is not null)
                sentTokens.TryAdd(id.Value, token);
        }

        var members = users
            .Where(u => u.Id != meId)
            .Select(u => new BoardFilterMemberModel
            {
                Token = sentTokens.GetValueOrDefault(u.Id) ?? u.Id.Encrypt(),
                Name = u.Name,
                PhotoUrl = photos.GetValueOrDefault(u.Id),
                HasAccess = u.HasAccess,
                Selected = sentTokens.ContainsKey(u.Id)
            })
            .ToList();

        var meSelected = meId != 0 && sentTokens.ContainsKey(meId);
        chosenIds = users.Where(u => u.Id != meId && sentTokens.ContainsKey(u.Id)).Select(u => u.Id).ToHashSet();
        if (meSelected)
            chosenIds.Add(meId);

        var due = BoardFilterModel.DueOptions
            .Select(o => o.Key)
            .FirstOrDefault(k => k.Length > 0 && string.Equals(k, request.Due, StringComparison.OrdinalIgnoreCase));
        DateOnly? from = null, to = null;
        if (due == BoardFilterModel.DueCustom)
        {
            from = ParseDate(request.From);
            to = ParseDate(request.To);
            if (from > to)
                from = to = null;
            if (from is null && to is null)
                due = null;   // a custom range without dates filters nothing
        }

        var statuses = BoardFilterModel.StatusOptions
            .Select(o => o.Key)
            .Where(k => (request.Status ?? []).Contains(k, StringComparer.OrdinalIgnoreCase))
            .ToList();

        return new BoardFilterModel
        {
            MeToken = sentTokens.GetValueOrDefault(meId) ?? meId.Encrypt(),
            MeSelected = meSelected,
            Members = members,
            Unassigned = request.Unassigned == "1",
            Due = due,
            From = from,
            To = to,
            Statuses = statuses,
            Unfinished = request.Unfinished == "1"
        };
    }

    private static DateOnly? ParseDate(string? value) =>
        DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null;

    // Groups AND together; the values inside a group OR together.
    private static bool Matches(TaskItemDto task, BoardFilterModel filter, HashSet<int> chosenIds,
        IEnumerable<int> taskAssigneeIds, BoardChecklistCountDto? checklists)
    {
        if (filter.HasAssignee)
        {
            var ids = taskAssigneeIds.ToList();
            if (!((filter.Unassigned && ids.Count == 0) || ids.Any(chosenIds.Contains)))
                return false;
        }

        if (filter.Due is not null)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            DateOnly? dueDate = task.DueDate is { } d ? DateOnly.FromDateTime(d) : null;
            var isDone = task.StatusName.Equals("Done", StringComparison.OrdinalIgnoreCase);
            var match = filter.Due switch
            {
                BoardFilterModel.DueOverdue => dueDate < today && !isDone,
                BoardFilterModel.DueToday => dueDate == today,
                BoardFilterModel.DueWeek => dueDate >= today && dueDate <= today.AddDays(6),
                BoardFilterModel.DueNone => dueDate is null,
                _ => dueDate is not null && (filter.From is null || dueDate >= filter.From) && (filter.To is null || dueDate <= filter.To)
            };
            if (!match)
                return false;
        }

        if (filter.Statuses.Count > 0
            && !BoardFilterModel.StatusOptions.Any(o => filter.Statuses.Contains(o.Key) && task.StatusName.Equals(o.Value, StringComparison.OrdinalIgnoreCase)))
            return false;

        if (filter.Unfinished && !(checklists is { Total: > 0 } && checklists.Done < checklists.Total))
            return false;

        return true;
    }

    private GetBoardTaskModel ToTaskModel(TaskItemDto task, IReadOnlyList<TaskAssigneeModel> assignees, BoardChecklistCountDto? checklists)
    {
        return new GetBoardTaskModel
        {
            Token = task.Id.Encrypt(),
            Title = task.Title,
            Description = task.Description,
            StatusName = task.StatusName,
            Assignees = assignees,
            DueDate = task.DueDate,
            ImageCount = task.ImageCount,
            CommentCount = task.CommentCount,
            ChecklistDone = checklists?.Done ?? 0,
            ChecklistTotal = checklists?.Total ?? 0,
            FirstImageUrl = _dms.GetUrl(DmsFolder.Task, task.FirstImageName)
        };
    }

    private async Task<IReadOnlyList<BoardColumnDto>> GetBoardColumnsAsync(int teamId, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", _userContext.UserId);
        parameters.Add("@TeamId", teamId);
        return await _db.QueryAsync<BoardColumnDto>("dbo.GetAllBoardsWithColumns", parameters, ct);
    }

    private async Task<IReadOnlyList<TaskItemDto>> GetAllTasksAsync(int boardId, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@BoardId", boardId);
        return await _db.QueryAsync<TaskItemDto>("dbo.GetAllTasksWithDetails", parameters, ct);
    }

    private async Task<IReadOnlyList<BoardChecklistCountDto>> GetBoardChecklistCountsAsync(int boardId, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@BoardId", boardId);
        return await _db.QueryAsync<BoardChecklistCountDto>("dbo.GetBoardChecklistCounts", parameters, ct);
    }

    // The people the task form offers for this board.
    private async Task<IReadOnlyList<UserOptionDto>> GetUserOptionsAsync(int boardId, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@BoardId", boardId);
        parameters.Add("@TaskId", null);
        parameters.Add("@UserId", _userContext.UserId);
        return await _db.QueryAsync<UserOptionDto>("dbo.GetUsersForDropdown", parameters, ct);
    }

    private async Task<IReadOnlyList<TaskAssigneeDto>> GetTaskAssigneesAsync(int? taskId, int? boardId, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@TaskId", taskId);
        parameters.Add("@BoardId", boardId);
        return await _db.QueryAsync<TaskAssigneeDto>("dbo.GetTaskAssignees", parameters, ct);
    }
}
