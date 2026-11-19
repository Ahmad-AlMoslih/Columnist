using Columnist.Users.Features.Profile.Queries.GetProfile.Models;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Users.Features.Profile.Queries.GetProfile;

// The signed-in user's own profile. TeamToken = the team cookie (picks the "Role · Team" line).
public sealed record GetProfileQuery(string? TeamToken = null) : IRequest<ApiResponse<GetProfileModel>>;
