namespace Columnist.Auth.Security.Sessions;

public interface ISessionVersionService
{
    Task<bool> IsCurrentAsync(int userId, int sessionVersion, CancellationToken ct = default);
}
