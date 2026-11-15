using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Teams.Features.BoardAccess.Commands.RemoveBoardMember;

public sealed record RemoveBoardMemberCommand(string BoardToken, string MemberToken) : IRequest<ApiResponse<bool>>;
