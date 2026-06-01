using Abstractions.Interfaces.Mail;
using Localization.Abstractions.Models;
using System.Net;

namespace Main.Application.EmailMessages;

public class PasswordResetEmailMessage : IEmailMessage
{
    private readonly Uri _resetUrl;
    private readonly Locale _locale;

    public string Subject => GetContent().Subject;
    public string To { get; }

    public PasswordResetEmailMessage(
        string to,
        Uri resetUrl,
        Locale? locale = null)
    {
        To = to;
        _resetUrl = resetUrl;
        _locale = locale ?? new Locale("EN");
    }

    public string GetHtmlBody()
    {
        var resetUrl = WebUtility.HtmlEncode(_resetUrl.ToString());
        var content = GetContent();

        return $"""
                 <!doctype html>
                 <html lang="{content.HtmlLang}">
                 <head>
                     <meta charset="utf-8">
                     <meta name="viewport" content="width=device-width, initial-scale=1">
                     <title>{content.Subject}</title>
                 </head>
                 <body style="margin:0;padding:0;background:#f3f4f6;font-family:Arial,Helvetica,sans-serif;color:#111827;">
                     <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:#f3f4f6;padding:32px 16px;">
                         <tr>
                             <td align="center">
                                 <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:560px;background:#ffffff;border-radius:8px;overflow:hidden;border:1px solid #e5e7eb;">
                                     <tr>
                                         <td style="padding:32px 32px 24px;">
                                             <h1 style="margin:0 0 16px;font-size:24px;line-height:32px;font-weight:700;color:#111827;">{content.Title}</h1>
                                             <p style="margin:0 0 16px;font-size:16px;line-height:24px;color:#374151;">
                                                 {content.Intro}
                                             </p>
                                             <p style="margin:0 0 24px;font-size:16px;line-height:24px;color:#374151;">
                                                 {content.Description}
                                             </p>
                                             <table role="presentation" cellpadding="0" cellspacing="0" style="margin:0 0 24px;">
                                                 <tr>
                                                     <td style="border-radius:6px;background:#2563eb;">
                                                         <a href="{resetUrl}" style="display:inline-block;padding:12px 20px;font-size:16px;line-height:20px;font-weight:700;color:#ffffff;text-decoration:none;border-radius:6px;">
                                                             {content.Button}
                                                         </a>
                                                     </td>
                                                 </tr>
                                             </table>
                                             <p style="margin:0 0 8px;font-size:14px;line-height:20px;color:#6b7280;">
                                                 {content.Fallback}
                                             </p>
                                             <p style="margin:0;font-size:14px;line-height:20px;word-break:break-all;">
                                                 <a href="{resetUrl}" style="color:#2563eb;text-decoration:underline;">{resetUrl}</a>
                                             </p>
                                         </td>
                                     </tr>
                                     <tr>
                                         <td style="padding:20px 32px;background:#f9fafb;border-top:1px solid #e5e7eb;">
                                             <p style="margin:0;font-size:13px;line-height:20px;color:#6b7280;">
                                                 {content.Ignore}
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

    private EmailContent GetContent()
    {
        return _locale == new Locale("RU")
            ? new EmailContent(
                "ru",
                "Восстановление пароля",
                "Восстановление пароля",
                "Мы получили запрос на восстановление пароля для вашей учетной записи.",
                "Нажмите на кнопку ниже, чтобы задать новый пароль. Ссылка действительна ограниченное время.",
                "Восстановить пароль",
                "Если кнопка не работает, скопируйте и вставьте эту ссылку в браузер:",
                "Если вы не запрашивали восстановление пароля, просто проигнорируйте это письмо.")
            : new EmailContent(
                "en",
                "Password reset",
                "Reset your password",
                "We received a request to reset the password for your account.",
                "Use the button below to choose a new password. This link is valid for a limited time.",
                "Reset password",
                "If the button does not work, copy and paste this link into your browser:",
                "If you did not request a password reset, you can safely ignore this email.");
    }

    private sealed record EmailContent(
        string HtmlLang,
        string Subject,
        string Title,
        string Intro,
        string Description,
        string Button,
        string Fallback,
        string Ignore);
}
