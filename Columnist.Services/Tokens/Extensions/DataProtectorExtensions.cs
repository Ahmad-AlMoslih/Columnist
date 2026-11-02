using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace Columnist.Services.Tokens.Extensions;

public static class DataProtectorExtensions
{
    public static string Encrypt<T>(this IDataProtector protector, T value)
        => protector.Protect(JsonSerializer.Serialize(value));

    public static bool TryDecrypt<T>(this IDataProtector protector, string? token, [MaybeNullWhen(false)] out T value)
    {
        value = default;
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            var result = JsonSerializer.Deserialize<T>(protector.Unprotect(token));
            if (result is null)
                return false;

            value = result;
            return true;
        }
        catch (Exception ex) when (ex is CryptographicException or JsonException)
        {
            return false;   // tampered or invalid token
        }
    }
}
