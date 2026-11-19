using System.Net;
using Columnist.Auth.Security.PasswordResets;
using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Email;
using Columnist.Utilities.Response;
using Dapper;
using MediatR;

namespace Columnist.Users.Features.Users.Commands.ForgotPassword;

public sealed class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, ApiResponse<bool>>
{
    private const string Subject = "Reset your Columnist password";

    private readonly IDbExecutor _db;
    private readonly IPasswordResetTokenService _resetTokens;
    private readonly IEmailSender _email;

    public ForgotPasswordHandler(IDbExecutor db, IPasswordResetTokenService resetTokens, IEmailSender email)
    {
        _db = db;
        _resetTokens = resetTokens;
        _email = email;
    }

    public async Task<ApiResponse<bool>> Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var token = _resetTokens.Generate();

        // No row = unknown email, or an email already sent in the last 2 minutes.
        var parameters = new DynamicParameters();
        parameters.Add("@Email", request.Email.Trim());
        parameters.Add("@TokenHash", _resetTokens.Hash(token));
        var recipient = await _db.QuerySingleOrDefaultAsync<PasswordResetRecipientDto>("dbo.CreatePasswordResetToken", parameters, ct);
        if (recipient is null)
            return ApiResponse<bool>.Success(true);

        var link = request.ResetPageUrl + "?token=" + Uri.EscapeDataString(token);
        var sent = await _email.SendAsync(recipient.Email, recipient.FullName, Subject, HtmlBody(recipient.FullName, link), TextBody(recipient.FullName, link), ct);

        if (!sent)
        {
            // Remove the unsent token so the user can ask again straight away.
            var deleteParameters = new DynamicParameters();
            deleteParameters.Add("@TokenId", recipient.TokenId);
            await _db.ExecuteAsync("dbo.DeletePasswordResetToken", deleteParameters, CancellationToken.None);
        }

        return ApiResponse<bool>.Success(true);
    }

    private static string HtmlBody(string fullName, string link)
    {
        var name = WebUtility.HtmlEncode(fullName);
        var href = WebUtility.HtmlEncode(link);
        return $"""
            <!DOCTYPE html>
            <html lang="en">
            <body style="margin:0;padding:24px;background:#F7F5F0;font-family:'Plus Jakarta Sans',Arial,sans-serif;color:#1F2A44;">
              <div style="max-width:480px;margin:0 auto;background:#FFFFFF;border:1px solid #E7E1D6;border-radius:16px;overflow:hidden;">
                <div style="height:6px;background:#C8553D;"></div>
                <div style="padding:26px 28px 28px;">
                  <h1 style="margin:0 0 12px;font-size:22px;">Reset your password</h1>
                  <p style="margin:0 0 20px;font-size:15px;line-height:1.5;">Hi {name}, use the button below to set a new Columnist password.</p>
                  <p style="margin:0 0 20px;"><a href="{href}" style="display:inline-block;padding:12px 20px;border-radius:12px;background:#C8553D;color:#FFFFFF;font-weight:700;text-decoration:none;">Reset password</a></p>
                  <p style="margin:0 0 8px;font-size:14px;color:#6B6459;">This link expires in 30 minutes and works once.</p>
                  <p style="margin:0;font-size:14px;color:#6B6459;">If you didn't ask for this, ignore this email.</p>
                </div>
              </div>
            </body>
            </html>
            """;
    }

    private static string TextBody(string fullName, string link) => $"""
        Hi {fullName},

        Use this link to set a new Columnist password:
        {link}

        This link expires in 30 minutes and works once.

        If you didn't ask for this, ignore this email.
        """;
}
