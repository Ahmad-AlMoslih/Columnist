namespace Columnist.Contracts.Dtos;

public class ResetPasswordFormDto
{
    public string? Token { get; set; }
    public string? Username { get; set; }   // display only: always reloaded from the token, never read from the post
    public string? NewPassword { get; set; }
    public string? ConfirmPassword { get; set; }
}
