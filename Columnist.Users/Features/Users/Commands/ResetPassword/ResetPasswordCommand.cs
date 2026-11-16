using Columnist.Auth.Authorization;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Users.Features.Users.Commands.ResetPassword;

// Sets a new password from a reset link and signs the user out everywhere.
// NotFound when the token is unknown, used or expired.
[AllowAnonymousRequest]
public sealed record ResetPasswordCommand(string Token, string NewPassword, string ConfirmPassword) : IRequest<ApiResponse<bool>>;
