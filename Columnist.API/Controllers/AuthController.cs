using Columnist.API.Extensions;
using Columnist.Auth.Features.Users.Commands.Login;
using Columnist.Auth.Features.Users.Commands.Logout;
using Columnist.Auth.Features.Users.Commands.RefreshToken;
using Columnist.Contracts.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Columnist.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        return (await _mediator.Send(new LoginCommand(request.Username ?? string.Empty, request.Password ?? string.Empty), ct)).ToActionResult();
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken ct)
    {
        return (await _mediator.Send(new RefreshTokenCommand(request.RefreshToken ?? string.Empty), ct)).ToActionResult();
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken ct)
    {
        return (await _mediator.Send(new LogoutCommand(request.RefreshToken), ct)).ToActionResult();
    }
}
