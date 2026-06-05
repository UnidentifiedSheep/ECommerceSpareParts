using Abstractions.Interfaces.Mail;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Net.Sockets;
using Polly;
using Polly.Retry;

namespace Mail;

public class EmailSender(
    IOptions<MailOptions> options
    ) : IEmailSender
{
    public async Task SendAsync(
        IEmailMessage message,
        CancellationToken token = default)
    {
        await SendBatchAsync([message], token);
    }

    public async Task SendBatchAsync(
        IEnumerable<IEmailMessage> messages,
        CancellationToken token = default)
    {
        var opt = options.Value;
        var maxBatchSize = Math.Max(1, opt.MaxBatchSize);

        var chunks = messages.Chunk(maxBatchSize)
            .Select(x => x.Select(z => BuildMessage(z, opt)).ToArray())
            .ToList();

        foreach (var chunk in chunks)
        {
            await WithRconAsync(async client =>
            {
                foreach (var message in chunk)
                    await SendWithRetryAsync(client, message, opt, token);
            }, token);

            await Task.Delay(opt.BatchDelay, token);
        }
    }

    private async Task WithRconAsync(
        Func<SmtpClient, Task> func,
        CancellationToken token = default)
    {
        using var client = new SmtpClient();
        await EnsureConnectedAsync(client, options.Value, token);

        await func(client);

        if (client.IsConnected)
            await client.DisconnectAsync(true, token);
    }

    private static async Task SendWithRetryAsync(
        SmtpClient client,
        MimeMessage message,
        MailOptions opt,
        CancellationToken token)
    {
        var retryPipeline = BuildRetryPipeline(opt);
        var state = new SendMessageState(client, message, opt);

        await retryPipeline.ExecuteAsync(SendMessageAsync, state, token);
    }

    private static async ValueTask SendMessageAsync(
        SendMessageState state,
        CancellationToken token)
    {
        try
        {
            await EnsureConnectedAsync(state.Client, state.Options, token);
            await state.Client.SendAsync(state.Message, token);
        }
        catch (Exception ex) when (IsTransient(ex))
        {
            await DisconnectSilentlyAsync(state.Client);
            throw;
        }
    }

    private static ResiliencePipeline BuildRetryPipeline(MailOptions opt)
    {
        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = Math.Max(0, opt.MaxRetryAttempts - 1),
                Delay = opt.RetryDelay,
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder()
                    .Handle<Exception>(IsTransient)
            })
            .Build();
    }

    private static async Task EnsureConnectedAsync(
        SmtpClient client,
        MailOptions opt,
        CancellationToken token)
    {
        if (client is { IsConnected: true, IsAuthenticated: true })
            return;

        if (client.IsConnected)
            await client.DisconnectAsync(true, token);

        await client.ConnectAsync(
            opt.Host,
            opt.Port,
            opt.SecureSocket,
            token);

        await client.AuthenticateAsync(opt.Username, opt.Password, token);
    }

    private static bool IsTransient(Exception ex)
    {
        return ex switch
        {
            SmtpCommandException smtpEx => IsTransientStatusCode(smtpEx.StatusCode),
            SmtpProtocolException => true,
            IOException => true,
            SocketException => true,
            TimeoutException => true,
            _ => false
        };
    }

    private static bool IsTransientStatusCode(SmtpStatusCode statusCode)
    {
        var code = (int)statusCode;
        return code is >= 400 and < 500;
    }

    private static async Task DisconnectSilentlyAsync(SmtpClient client)
    {
        if (!client.IsConnected)
            return;

        try
        {
            await client.DisconnectAsync(true, CancellationToken.None);
        }
        catch
        {
            // The next retry creates a fresh SMTP session.
        }
    }

    private static MimeMessage BuildMessage(
        IEmailMessage message,
        MailOptions opt)
    {
        var toSend = new MimeMessage
        {
            Subject = message.Subject,
            Body = new BodyBuilder
            {
                HtmlBody = message.GetHtmlBody()
            }.ToMessageBody()
        };

        toSend.From.Add(new MailboxAddress(opt.FromName, opt.FromEmail));
        toSend.To.Add(MailboxAddress.Parse(message.To));

        return toSend;
    }

    private sealed record SendMessageState(
        SmtpClient Client,
        MimeMessage Message,
        MailOptions Options);
}
