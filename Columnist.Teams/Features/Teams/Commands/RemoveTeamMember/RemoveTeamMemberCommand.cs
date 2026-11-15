using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Teams.Features.Teams.Commands.RemoveTeamMember;

public sealed record RemoveTeamMemberCommand(string TeamToken, string MemberToken) : IRequest<ApiResponse<bool>>;
