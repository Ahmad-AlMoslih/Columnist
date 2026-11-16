namespace Columnist.Auth.Security.PasswordResets;

public interface IPasswordResetTokenService
{
    // 32 random bytes, URL-safe Base64. Only ever sent in the email; never stored or logged.
    string Generate();

    // SHA-256 as 64 hex characters: the only form stored in dbo.PasswordResetTokens.
    string Hash(string token);
}
