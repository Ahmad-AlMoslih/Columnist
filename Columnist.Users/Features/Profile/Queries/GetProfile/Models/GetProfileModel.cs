namespace Columnist.Users.Features.Profile.Queries.GetProfile.Models;

public sealed record GetProfileModel
{
    public string FullName { get; init; } = "";
    public string Initials { get; init; } = "";
    public string? Username { get; init; }
    public string Email { get; init; } = "";
    public string? PhotoUrl { get; init; }

    // The current team (null = no active team).
    public string? TeamName { get; init; }
    public string? RoleLabel { get; init; }   // "Super Admin" or "Member"
}
