using Microsoft.AspNetCore.DataProtection;

namespace Columnist.Services.Tokens.Extensions;

public static class TokenExtensions
{
    private static IDataProtector? _protector;

    public static void Configure(IDataProtectionProvider provider) =>
    _protector = provider.CreateProtector("Columnist.GeneralPurpose");   // same purpose as before: existing tokens keep working

    public static string Encrypt<T>(this T value) => Protector.Encrypt(value);

    public static T? Decrypt<T>(this string? token) where T : struct =>
        Protector.TryDecrypt<T>(token, out var value) ? value : null;

    private static IDataProtector Protector =>
        _protector ?? throw new InvalidOperationException("TokenExtensions.Configure was not called at startup.");
}