using MediatR;
using Columnist.Utilities.Response;

namespace Columnist.Tasks.Features.Tasks.Queries.GetAllTasks;

public sealed record GetAllTasksQuery : IRequest<ApiResponse<IReadOnlyList<GetAllTasksModel>>>;
