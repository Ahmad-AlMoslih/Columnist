using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

namespace Columnist.Auth.Security.PasswordResets;

public sealed class PasswordResetTokenService : IPasswordResetTokenService
{
    public string Generate() =>
        Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));

    public string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
