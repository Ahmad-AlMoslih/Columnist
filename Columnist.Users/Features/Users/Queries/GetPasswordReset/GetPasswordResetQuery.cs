using Columnist.Auth.Authorization;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Users.Features.Users.Queries.GetPasswordReset;

// Returns the username a reset token belongs to; NotFound when the token is unknown, used or expired.
[AllowAnonymousRequest]
public sealed record GetPasswordResetQuery(string Token) : IRequest<ApiResponse<string>>;
