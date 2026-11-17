namespace Persistence.Listening;

public interface IChannelNotifier
{
	Task NotifyAsync(
		string channel,
		string? payload = null,
		CancellationToken cancellationToken = default);
}
