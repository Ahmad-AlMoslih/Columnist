namespace Columnist.Contracts.Dtos;

public class LoginFormDto
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? ReturnUrl { get; set; }
}
