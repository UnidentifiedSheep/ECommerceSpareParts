using System.Net.Sockets;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Notification.Core;
using Notification.Options;
using Polly;
using Polly.Retry;

namespace Notification.Channels.Email;

public interface IEmailSender
{
	Task<SendResult> SendAsync(EmailMessage message, CancellationToken token = default);

	Task<IReadOnlyList<SendResult>> SendBatchAsync(
		IEnumerable<EmailMessage> messages,
		CancellationToken token = default);
}


public class EmailSender(IOptions<EmailChannelOptions> options) : IEmailSender
{
	public async Task<SendResult> SendAsync(EmailMessage message, CancellationToken token = default)
	{
		ArgumentNullException.ThrowIfNull(message);
		var results = await SendBatchAsync([message], token);
		return results[0];
	}

	public async Task<IReadOnlyList<SendResult>> SendBatchAsync(
		IEnumerable<EmailMessage> messages,
		CancellationToken token = default)
	{
		ArgumentNullException.ThrowIfNull(messages);
		token.ThrowIfCancellationRequested();

		var opt = options.Value;
		var maxBatchSize = Math.Max(1, opt.MaxBatchSize);
		var chunks = messages.Chunk(maxBatchSize).ToArray();
		var results = new List<SendResult>();

		for (var chunkIndex = 0; chunkIndex < chunks.Length; chunkIndex++)
		{
			if (chunkIndex > 0)
				await Task.Delay(opt.BatchDelay, token);

			using var client = new SmtpClient();
			foreach (var message in chunks[chunkIndex])
			{
				token.ThrowIfCancellationRequested();
				try
				{
					await SendWithRetryAsync(client, BuildMessage(message, opt), opt, token);
					results.Add(SendResult.Success());
				}
				catch (OperationCanceledException) when (token.IsCancellationRequested)
				{
					throw;
				}
				catch (Exception ex)
				{
					results.Add(SendResult.Failure(
						string.IsNullOrWhiteSpace(ex.Message) ? ex.GetType().Name : ex.Message));
				}
			}

			await DisconnectSilentlyAsync(client);
		}

		return results;
	}

	private static async Task SendWithRetryAsync(
		SmtpClient client,
		MimeMessage message,
		EmailChannelOptions opt,
		CancellationToken token)
	{
		var retryPipeline = BuildRetryPipeline(opt);
		var state = new SendMessageState(
			client,
			message,
			opt);

		await retryPipeline.ExecuteAsync(
			SendMessageAsync,
			state,
			token);
	}

	private static async ValueTask SendMessageAsync(SendMessageState state, CancellationToken token)
	{
		try
		{
			await EnsureConnectedAsync(
				state.Client,
				state.Options,
				token);
			await state.Client.SendAsync(state.Message, token);
		}
		catch (Exception ex) when (IsTransient(ex))
		{
			await DisconnectSilentlyAsync(state.Client);
			throw;
		}
	}

	private static ResiliencePipeline BuildRetryPipeline(EmailChannelOptions opt)
	{
		return new ResiliencePipelineBuilder()
			.AddRetry(
				new RetryStrategyOptions
				{
					MaxRetryAttempts = Math.Max(0, opt.MaxRetryAttempts - 1),
					Delay = opt.RetryDelay,
					BackoffType = DelayBackoffType.Exponential,
					ShouldHandle = new PredicateBuilder().Handle<Exception>(IsTransient)
				})
			.Build();
	}

	private static async Task EnsureConnectedAsync(
		SmtpClient client,
		EmailChannelOptions opt,
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

		await client.AuthenticateAsync(
			opt.Username,
			opt.Password,
			token);
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

	private static MimeMessage BuildMessage(EmailMessage message, EmailChannelOptions opt)
	{
		var toSend = new MimeMessage
		{
			Subject = message.Title,
			Body = new BodyBuilder
			{
				HtmlBody = message.Body
			}.ToMessageBody()
		};

		toSend.From.Add(new MailboxAddress(opt.FromName, opt.FromEmail));
		toSend.To.Add(MailboxAddress.Parse(message.To));

		return toSend;
	}

	private sealed record SendMessageState(SmtpClient Client, MimeMessage Message, EmailChannelOptions Options);
}
