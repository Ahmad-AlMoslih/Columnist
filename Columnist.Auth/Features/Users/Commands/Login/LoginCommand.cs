using Columnist.Contracts.Dtos;
using MediatR;
using Columnist.Utilities.Response;
using Columnist.Auth.Authorization;

namespace Columnist.Auth.Features.Users.Commands.Login;

[AllowAnonymousRequest]
public sealed record LoginCommand(string Username, string Password) : IRequest<ApiResponse<LoginResultDto>>;
