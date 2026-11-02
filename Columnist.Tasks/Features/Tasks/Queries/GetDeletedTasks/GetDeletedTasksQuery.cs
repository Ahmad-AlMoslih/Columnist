using Columnist.Auth.Authorization;
using Columnist.Tasks.Features.Tasks.Queries.GetDeletedTasks.Models;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Tasks.Features.Tasks.Queries.GetDeletedTasks;

[RequireRole(UserRoles.Admin)]
public sealed record GetDeletedTasksQuery(string? Search = null) : IRequest<ApiResponse<GetDeletedTasksModel>>;
