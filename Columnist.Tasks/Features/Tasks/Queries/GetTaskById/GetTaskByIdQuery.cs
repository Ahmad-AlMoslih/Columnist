using MediatR;
using Columnist.Utilities.Response;

namespace Columnist.Tasks.Features.Tasks.Queries.GetTaskById;

public sealed record GetTaskByIdQuery(string Token) : IRequest<ApiResponse<GetTaskByIdModel>>;
