namespace Columnist.Services.Email;

public interface IEmailSender
{
    // Sends one email with an HTML body and a plain-text alternative.
    // Returns false (and logs the error) when sending fails; never throws for SMTP errors.
    Task<bool> SendAsync(string toAddress, string toName, string subject, string htmlBody, string textBody, CancellationToken ct = default);
}
