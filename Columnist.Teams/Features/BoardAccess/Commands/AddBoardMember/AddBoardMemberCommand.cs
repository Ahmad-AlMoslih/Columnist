using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Teams.Features.BoardAccess.Commands.AddBoardMember;

public sealed record AddBoardMemberCommand(string BoardToken, string MemberToken) : IRequest<ApiResponse<bool>>;
