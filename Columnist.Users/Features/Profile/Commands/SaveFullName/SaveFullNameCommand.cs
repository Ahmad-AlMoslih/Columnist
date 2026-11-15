using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Users.Features.Profile.Commands.SaveFullName;

// Saves the signed-in user's full name. Returns a new JWT carrying the new name (same session version).
public sealed record SaveFullNameCommand(string FullName) : IRequest<ApiResponse<string>>
{
    public const int FullNameMaxLength = 100;
}
