using Columnist.Contracts.Dtos;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Users.Features.Profile.Commands.ChangePassword;

// Changes the signed-in user's password and signs out their other sessions.
// Returns new tokens for the current session.
public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword, string ConfirmPassword) : IRequest<ApiResponse<LoginResultDto>>
{
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 72;   // BCrypt ignores anything longer
}
