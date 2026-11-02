using Columnist.Auth.Authorization;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Tasks.Features.Tasks.Commands.RestoreTask;

[RequireRole(UserRoles.Admin)]
public sealed record RestoreTaskCommand(string Token) : IRequest<ApiResponse<bool>>;
