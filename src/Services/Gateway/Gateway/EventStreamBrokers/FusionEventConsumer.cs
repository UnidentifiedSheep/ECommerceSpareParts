using MassTransit;

namespace Gateway.EventStreamBrokers;

public sealed class FusionEventConsumer<TEvent>(
	IEventHub hub,
	IEventRegistry registry)
	: IConsumer<TEvent> where TEvent : class
{
	public Task Consume(ConsumeContext<TEvent> context)
	{
		var userId = registry.Get<TEvent>().ExtractUserId(context.Message);
		var audience = userId == null ? EventAudience.Global : EventAudience.ForUser(userId.Value);

		hub.Publish(context.Message, audience);

		return Task.CompletedTask;
	}
}
