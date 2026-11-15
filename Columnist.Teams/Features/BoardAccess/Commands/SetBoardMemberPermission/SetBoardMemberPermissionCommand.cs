using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Teams.Features.BoardAccess.Commands.SetBoardMemberPermission;

public sealed record SetBoardMemberPermissionCommand(string BoardToken, string MemberToken, string PermissionName, bool Grant) : IRequest<ApiResponse<bool>>;
