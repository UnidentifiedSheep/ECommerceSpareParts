namespace Persistence.Listening;

public interface IChannelListenerFactory
{
	Task<IChannelListener> ListenAsync(string channel, CancellationToken cancellationToken = default);
}
