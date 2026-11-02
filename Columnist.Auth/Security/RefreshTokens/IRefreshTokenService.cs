namespace Columnist.Auth.Security.RefreshTokens;

public interface IRefreshTokenService
{
    string Generate();
    string Hash(string token);
    DateTime GetExpiry();
}
