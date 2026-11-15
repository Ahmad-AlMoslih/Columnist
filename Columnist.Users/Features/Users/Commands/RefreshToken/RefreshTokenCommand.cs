using Columnist.Auth.Authorization;
using Columnist.Contracts.Dtos;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Users.Features.Users.Commands.RefreshToken;

[AllowAnonymousRequest]
public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<ApiResponse<LoginResultDto>>;
