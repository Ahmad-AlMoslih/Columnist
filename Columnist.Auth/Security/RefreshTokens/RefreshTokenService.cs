using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Columnist.Auth.Security.Jwt;

namespace Columnist.Auth.Security.RefreshTokens;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly JwtSettings _settings;

    public RefreshTokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public string Generate() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

    public string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    public DateTime GetExpiry() =>
        DateTime.Now.AddDays(_settings.RefreshTokenDays);
}