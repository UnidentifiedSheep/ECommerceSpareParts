namespace Persistence.Listening;

public interface IChannelListener : IAsyncDisposable
{
	string Channel { get; }

	IAsyncEnumerable<ChannelNotification> ReadAllAsync(CancellationToken cancellationToken = default);
}
