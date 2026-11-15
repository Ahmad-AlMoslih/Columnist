using Columnist.Users.Features.Profile.Queries.GetProfile.Models;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Users.Features.Profile.Queries.GetProfile;

// The signed-in user's own profile.
public sealed record GetProfileQuery : IRequest<ApiResponse<GetProfileModel>>;
