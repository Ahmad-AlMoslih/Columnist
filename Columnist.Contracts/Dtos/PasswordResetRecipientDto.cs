namespace Columnist.Contracts.Dtos;

// A row of dbo.CreatePasswordResetToken: the new token's Id and who to email.
public sealed record PasswordResetRecipientDto
{
    public int TokenId { get; init; }
    public string Email { get; init; } = "";
    public string FullName { get; init; } = "";
}
