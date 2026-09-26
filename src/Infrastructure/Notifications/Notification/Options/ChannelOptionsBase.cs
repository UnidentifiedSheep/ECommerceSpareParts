using System.ComponentModel.DataAnnotations;

namespace Notification.Options;

public abstract record ChannelOptionsBase
{
	public required TimeSpan DelayBeforeBatch { get; init; } = TimeSpan.FromSeconds(30);

	public required int MaxAttempts { get; init; } = 3;

	public required int BatchSize { get; init; } = 100;
}
