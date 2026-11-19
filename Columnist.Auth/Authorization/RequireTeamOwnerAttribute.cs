namespace Columnist.Auth.Authorization;

// The user must be the Super Admin (owner) of the request's team, or of the team that owns the request's board.
// The request must implement ITeamRequest. An empty scope or a token that doesn't decrypt is left to the handler.
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class RequireTeamOwnerAttribute : Attribute
{
}
