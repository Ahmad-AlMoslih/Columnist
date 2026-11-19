using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Users.Features.Profile.Commands.SaveProfile;

// Saves the signed-in user's full name and username together. Returns a new JWT carrying them (same session version).
public sealed record SaveProfileCommand(string FullName, string Username) : IRequest<ApiResponse<string>>
{
    public const int FullNameMaxLength = 100;
    public const int UsernameMinLength = 3;
    public const int UsernameMaxLength = 50;
}
