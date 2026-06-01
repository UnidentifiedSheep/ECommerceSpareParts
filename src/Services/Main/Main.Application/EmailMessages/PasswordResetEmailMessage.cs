using Abstractions.Interfaces.Mail;
using System.Net;

namespace Main.Application.EmailMessages;

public class PasswordResetEmailMessage : IEmailMessage
{
    private readonly Uri _resetUrl;

    public string Subject => "Password reset";
    public string To { get; }

    public PasswordResetEmailMessage(
        string to,
        Uri resetUrl)
    {
        To = to;
        _resetUrl = resetUrl;
    }

    public string GetHtmlBody()
    {
        var resetUrl = WebUtility.HtmlEncode(_resetUrl.ToString());

        return $"""
                 <!doctype html>
                 <html lang="en">
                 <head>
                     <meta charset="utf-8">
                     <meta name="viewport" content="width=device-width, initial-scale=1">
                     <title>Password reset</title>
                 </head>
                 <body style="margin:0;padding:0;background:#f3f4f6;font-family:Arial,Helvetica,sans-serif;color:#111827;">
                     <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:#f3f4f6;padding:32px 16px;">
                         <tr>
                             <td align="center">
                                 <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:560px;background:#ffffff;border-radius:8px;overflow:hidden;border:1px solid #e5e7eb;">
                                     <tr>
                                         <td style="padding:32px 32px 24px;">
                                             <h1 style="margin:0 0 16px;font-size:24px;line-height:32px;font-weight:700;color:#111827;">Reset your password</h1>
                                             <p style="margin:0 0 16px;font-size:16px;line-height:24px;color:#374151;">
                                                 We received a request to reset the password for your account.
                                             </p>
                                             <p style="margin:0 0 24px;font-size:16px;line-height:24px;color:#374151;">
                                                 Use the button below to choose a new password. This link is valid for a limited time.
                                             </p>
                                             <table role="presentation" cellpadding="0" cellspacing="0" style="margin:0 0 24px;">
                                                 <tr>
                                                     <td style="border-radius:6px;background:#2563eb;">
                                                         <a href="{resetUrl}" style="display:inline-block;padding:12px 20px;font-size:16px;line-height:20px;font-weight:700;color:#ffffff;text-decoration:none;border-radius:6px;">
                                                             Reset password
                                                         </a>
                                                     </td>
                                                 </tr>
                                             </table>
                                             <p style="margin:0 0 8px;font-size:14px;line-height:20px;color:#6b7280;">
                                                 If the button does not work, copy and paste this link into your browser:
                                             </p>
                                             <p style="margin:0;font-size:14px;line-height:20px;word-break:break-all;">
                                                 <a href="{resetUrl}" style="color:#2563eb;text-decoration:underline;">{resetUrl}</a>
                                             </p>
                                         </td>
                                     </tr>
                                     <tr>
                                         <td style="padding:20px 32px;background:#f9fafb;border-top:1px solid #e5e7eb;">
                                             <p style="margin:0;font-size:13px;line-height:20px;color:#6b7280;">
                                                 If you did not request a password reset, you can safely ignore this email.
                                             </p>
                                         </td>
                                     </tr>
                                 </table>
                             </td>
                         </tr>
                     </table>
                 </body>
                 </html>
                 """;
    }
}
