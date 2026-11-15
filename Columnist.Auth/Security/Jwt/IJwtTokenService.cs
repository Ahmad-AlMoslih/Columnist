namespace Columnist.Auth.Security.Jwt;

public interface IJwtTokenService
{
    string CreateToken(int userId, string username, string fullName, int sessionVersion);
}
