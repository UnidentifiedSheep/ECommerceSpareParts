using System.Collections.Frozen;
using Contracts;

namespace Gateway.EventStreamBrokers;

public interface IEventRegistry
{
	FusionEventDescriptor Get<TEvent>();
	FusionEventDescriptor Get(string topic);
}

public sealed class EventRegistry : IEventRegistry
{
	private readonly FrozenDictionary<Type, FusionEventDescriptor> _descriptors;
	private readonly FrozenDictionary<string, FusionEventDescriptor> _byTopicDescriptors;

	public EventRegistry()
	{
		var dict = new Dictionary<Type, FusionEventDescriptor>();
		var byTopic = new Dictionary<string, FusionEventDescriptor>();

		Add<InAppNotificationCreatedEvent>(
			byType: dict,
			byTopic: byTopic,
			topic: "onNotificationCreated",
			mapper: @event => new
			{
				id = @event.Id
			},
			extractAudience: @event => EventAudience.ForUser(@event.UserId));

		_descriptors = dict.ToFrozenDictionary();
		_byTopicDescriptors = byTopic.ToFrozenDictionary();
	}

	private static void Add<TEvent>(
		Dictionary<Type, FusionEventDescriptor> byType,
		Dictionary<string, FusionEventDescriptor> byTopic,
		string topic,
		Func<TEvent, object> mapper,
		Func<TEvent, EventAudience> extractAudience)
	{
		var descriptor = new FusionEventDescriptor(
			topic,
			typeof(TEvent),
			value => mapper((TEvent)value),
			value => extractAudience((TEvent)value));

		byType.Add(typeof(TEvent), descriptor);
		byTopic.Add(topic, descriptor);
	}

	public FusionEventDescriptor Get<TEvent>()
		=> _descriptors.TryGetValue(typeof(TEvent), out var descriptor)
			? descriptor
			: throw new InvalidOperationException(
				$"Fusion event '{typeof(TEvent).FullName}' is not registered.");

	public FusionEventDescriptor Get(string topic)
		=> _byTopicDescriptors.TryGetValue(topic, out var descriptor)
			? descriptor
			: throw new InvalidOperationException(
				$"Fusion event topic '{topic}' is not registered.");
}
