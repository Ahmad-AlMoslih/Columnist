namespace Columnist.Auth.Authorization;

// A request about a specific team or board: AuthorizationBehavior checks [RequireTeamOwner] on THAT team.
public interface ITeamRequest
{
    TeamScope GetTeamScope();
}
