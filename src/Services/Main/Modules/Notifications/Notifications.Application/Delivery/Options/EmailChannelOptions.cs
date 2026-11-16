using System.ComponentModel.DataAnnotations;
using MailKit.Security;

namespace Notification.Options;

public record EmailChannelOptions : ChannelOptionsBase
{
	public const string SectionName = "Mail";

	[Required]
	public required string Host { get; init; }

	[Required]
	public required int Port { get; init; }

	[Required]
	public required string Username { get; init; }

	[Required]
	public required string Password { get; init; }

	[Required]
	public required string FromName { get; init; }

	[Required]
	public required string FromEmail { get; init; }

	public required int MaxBatchSize { get; init; } = 10;

	public required TimeSpan BatchDelay { get; init; } = TimeSpan.FromMilliseconds(300);

	public required int MaxRetryAttempts { get; init; } = 3;

	public required TimeSpan RetryDelay { get; init; } = TimeSpan.FromSeconds(1);

	public required SecureSocketOptions SecureSocket { get; init; } = SecureSocketOptions.Auto;
}
