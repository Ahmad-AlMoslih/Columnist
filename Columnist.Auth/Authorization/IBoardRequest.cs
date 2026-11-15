namespace Columnist.Auth.Authorization;

// A request about a specific board: AuthorizationBehavior checks its permission on THAT board.
public interface IBoardRequest
{
    BoardScope GetBoardScope();
}
