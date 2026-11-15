using Columnist.Auth.Authorization;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Users.Features.Users.Commands.Logout;

[AllowAnonymousRequest]
public sealed record LogoutCommand(string? RefreshToken) : IRequest<ApiResponse<bool>>;
