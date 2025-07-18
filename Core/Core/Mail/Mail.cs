using System.Text;
using Core.Mail.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Core.Mail;

public class Mail : IMail
{
    private readonly MailOptions _options;

    public Mail(IOptions<MailOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendMailAsync(string receiver ,string body = "" , string subject = "" ,HeaderList? headers = null ,CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        if (headers != null && headers.Any())
            foreach (var header in headers)
                message.Headers.Add(header);
        message.Subject = subject;
        message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };
        //TODO CHANGE ORGANISATION NAME
        message.From.Add(new MailboxAddress(_options.Username, _options.Username));
        message.Sender = new MailboxAddress(_options.Username, _options.Username);
        message.To.Add(new MailboxAddress(receiver, receiver));
        using var client = new SmtpClient();
        await client.ConnectAsync(_options.Smtp, _options.Port, true, cancellationToken);
        await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}