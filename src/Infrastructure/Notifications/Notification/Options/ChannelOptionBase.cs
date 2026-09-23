using System.ComponentModel.DataAnnotations;

namespace Notification.Options;

public abstract record ChannelOptionBase
{
	[Required]
	public required TimeSpan DelayBeforeBatch { get; init; } = TimeSpan.FromSeconds(30);
}
