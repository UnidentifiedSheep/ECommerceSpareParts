using HotChocolate.Fusion.Subscriptions;

namespace Gateway.EventStreamBrokers;

public class RabbitmqEventStreamBroker : IEventStreamBroker
{
	public IAsyncEnumerable<EventMessage> SubscribeAsync(
		ISubscriptionFieldContext context,
		string[] topics,
		string? cursor,
		CancellationToken cancellationToken)
	{

	}

	public ValueTask DisposeAsync() => throw new NotImplementedException();
}
