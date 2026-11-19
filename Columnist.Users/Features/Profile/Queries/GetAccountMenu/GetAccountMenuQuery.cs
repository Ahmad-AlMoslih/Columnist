using Columnist.Users.Features.Profile.Queries.GetAccountMenu.Models;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Users.Features.Profile.Queries.GetAccountMenu;

// What the account menu shows for the signed-in user. TeamToken = the team cookie (picks the subtitle's team).
public sealed record GetAccountMenuQuery(string? TeamToken = null) : IRequest<ApiResponse<GetAccountMenuModel>>;
