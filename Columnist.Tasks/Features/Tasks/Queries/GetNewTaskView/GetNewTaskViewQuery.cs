using Columnist.Auth.Authorization;
using Columnist.Tasks.Features.Tasks.Queries.GetTaskView.Models;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Tasks.Features.Tasks.Queries.GetNewTaskView;

// The task view in draft mode for "New task" / "+ Add task": ColumnToken = the clicked column, otherwise the board's first column.
[RequirePermission(Permission.TasksWrite)]
public sealed record GetNewTaskViewQuery(string? BoardToken, string? ColumnToken) : IRequest<ApiResponse<GetTaskViewModel>>, IBoardRequest
{
    public BoardScope GetBoardScope() => !string.IsNullOrEmpty(ColumnToken)
        ? new(ColumnToken: ColumnToken)
        : new(BoardToken: BoardToken);
}
