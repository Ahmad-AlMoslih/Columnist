namespace Columnist.Users.Features.Profile.Queries.GetAccountMenu.Models;

public sealed record GetAccountMenuModel
{
    public string? PhotoUrl { get; init; }

    // The current team (null = no active team, no subtitle).
    public string? TeamName { get; init; }
    public string? RoleLabel { get; init; }   // "Super Admin" or "Member"
}
