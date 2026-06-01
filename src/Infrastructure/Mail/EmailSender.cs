using Abstractions.Interfaces.Mail;
using Mail.Interface;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Mail;

public class EmailSender(
    IOptions<MailOptions> options
    ) : IEmailSender
{
    public async Task SendAsync(
        IEmailMessage message, 
        CancellationToken token = default)
    {
        var opt = options.Value;
        var toSend = new MimeMessage();
        toSend.From.Add(new MailboxAddress(opt.FromName, opt.FromEmail));
        toSend.To.Add(MailboxAddress.Parse(message.To));
        toSend.Subject = message.Subject;

        toSend.Body = new BodyBuilder
        {
            HtmlBody = message.GetHtmlBody()
        }.ToMessageBody();

        using var client = new SmtpClient();
        
        await client.ConnectAsync(
            opt.Host,
            opt.Port,
            opt.SecureSocket,
            token);
        
        await client.AuthenticateAsync(opt.Username, opt.Password, token);
        await client.SendAsync(toSend, token);
        await client.DisconnectAsync(true, token);
    }
}