using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Columnist.Services.Email;

// SMTP with STARTTLS (EnableSsl on a submission port such as 587 upgrades the connection with STARTTLS).
public sealed class EmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IOptions<EmailSettings> settings, ILogger<EmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<bool> SendAsync(string toAddress, string toName, string subject, string htmlBody, string textBody, CancellationToken ct = default)
    {
        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromAddress, _settings.FromName),
                Subject = subject,
                SubjectEncoding = Encoding.UTF8,
                Body = textBody,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = false
            };
            message.To.Add(new MailAddress(toAddress, toName));
            message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, MediaTypeNames.Text.Html));

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_settings.Username, _settings.Password)
            };

            await client.SendMailAsync(message, ct);
            return true;
        }
        catch (Exception ex)
        {
            // The body holds secrets (e.g. reset links), so only the subject is logged.
            _logger.LogError(ex, "Email \"{Subject}\" could not be sent.", subject);
            return false;
        }
    }
}
