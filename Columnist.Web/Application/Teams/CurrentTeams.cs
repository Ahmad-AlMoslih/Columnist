using Columnist.Teams.Features.Teams.Queries.GetMyTeams;
using Columnist.Teams.Features.Teams.Queries.GetMyTeams.Models;
using Columnist.Utilities.Response;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Columnist.Web.Application.Teams;

// The current user's teams (team cookie + GetMyTeamsQuery), loaded at most once per HTTP request. Registered as Scoped.
public sealed class CurrentTeams
{
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessor _http;
    private ApiResponse<GetMyTeamsModel>? _teams;

    public CurrentTeams(IMediator mediator, IHttpContextAccessor http)
    {
        _mediator = mediator;
        _http = http;
    }

    public async Task<ApiResponse<GetMyTeamsModel>> GetAsync(CancellationToken ct = default) =>
        _teams ??= await _mediator.Send(new GetMyTeamsQuery(TeamCookie.Get(_http.HttpContext!.Request)), ct);
}
