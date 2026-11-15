using System.Security.Cryptography;

namespace Columnist.Teams.Features.Teams;

// Codes look like K7Q4-MZ2P. No 0/O, 1/I/L to avoid mix-ups.
public static class InviteCodes
{
    private const string Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

    public static string New()
    {
        var raw = RandomNumberGenerator.GetString(Alphabet, 8);
        return $"{raw[..4]}-{raw[4..]}";
    }

    // Accepts "k7q4mz2p", "K7Q4 MZ2P", "K7Q4-MZ2P" … → "K7Q4-MZ2P"; null if it can't be a code.
    public static string? Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;
        var raw = new string(input.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        return raw.Length == 8 ? $"{raw[..4]}-{raw[4..]}" : null;
    }
}
