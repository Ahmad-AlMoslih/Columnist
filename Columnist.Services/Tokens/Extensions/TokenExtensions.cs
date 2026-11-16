using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;

namespace Columnist.Services.Tokens.Extensions;

public static class TokenExtensions
{
    private static IDataProtector? _protector;
    private static byte[]? _stableKeySecret;

    public static void Configure(IDataProtectionProvider provider, string? secret)
    {
        _protector = provider.CreateProtector("Columnist.GeneralPurpose");   // same purpose as before: existing tokens keep working
        if (!string.IsNullOrEmpty(secret))
            _stableKeySecret = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes("Columnist.StableKeys"));
    }

    public static string Encrypt<T>(this T value) => Protector.Encrypt(value);

    public static T? Decrypt<T>(this string? token) where T : struct =>
        Protector.TryDecrypt<T>(token, out var value) ? value : null;

    // Same id and purpose → same short key on every render (unlike Encrypt). A truncated HMAC: the id can't be read back.
    public static string ToStableKey(this int id, string purpose)
    {
        var secret = _stableKeySecret ?? throw new InvalidOperationException("TokenExtensions.Configure was not given a secret.");
        var hash = HMACSHA256.HashData(secret, Encoding.UTF8.GetBytes($"{purpose}:{id}"));
        return Convert.ToHexString(hash, 0, 8).ToLowerInvariant();
    }

    private static IDataProtector Protector =>
        _protector ?? throw new InvalidOperationException("TokenExtensions.Configure was not called at startup.");
}