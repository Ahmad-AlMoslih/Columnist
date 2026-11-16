namespace Columnist.Services.Email;

// The "Email" configuration section. Real values live in User Secrets.
public sealed class EmailSettings
{
    public string Host { get; init; } = "";
    public int Port { get; init; } = 587;
    public string Username { get; init; } = "";
    public string Password { get; init; } = "";
    public string FromAddress { get; init; } = "";
    public string FromName { get; init; } = "";
}
