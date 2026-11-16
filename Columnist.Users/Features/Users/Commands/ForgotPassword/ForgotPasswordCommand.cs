using Columnist.Auth.Authorization;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Users.Features.Users.Commands.ForgotPassword;

// Emails a password reset link when the username exists, has an email and wasn't sent one in the last 2 minutes.
// Succeeds the same way in every case, so the caller can't tell which usernames exist.
// ResetPageUrl = the absolute URL of the reset page, built from the current request; the token is added as ?token=.
[AllowAnonymousRequest]
public sealed record ForgotPasswordCommand(string Username, string ResetPageUrl) : IRequest<ApiResponse<bool>>;
