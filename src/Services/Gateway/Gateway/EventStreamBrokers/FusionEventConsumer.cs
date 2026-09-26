using MassTransit;

namespace Gateway.EventStreamBrokers;

public sealed class FusionEventConsumer<TEvent>(
	IEventHub hub,
	IEventRegistry registry)
	: IConsumer<TEvent> where TEvent : class
{
	public Task Consume(ConsumeContext<TEvent> context)
	{
		var audience = registry.Get<TEvent>().ResolveAudience(context.Message);
		hub.Publish(context.Message, audience);
		return Task.CompletedTask;
	}
}
