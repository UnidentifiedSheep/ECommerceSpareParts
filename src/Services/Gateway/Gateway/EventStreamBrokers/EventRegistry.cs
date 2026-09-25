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
	public static readonly EventRegistry Instance = new();

	public FrozenDictionary<Type, FusionEventDescriptor> ByTypeDescriptors { get; }
	public FrozenDictionary<string, FusionEventDescriptor> ByTopicDescriptors { get; }

	private EventRegistry()
	{
		var byType = new Dictionary<Type, FusionEventDescriptor>();
		var byTopic = new Dictionary<string, FusionEventDescriptor>();

		Add<InAppNotificationCreatedEvent>(
			byType: byType,
			byTopic: byTopic,
			topic: "onNotificationCreated",
			mapper: @event => new { id = @event.Id },
			resolveAudience: @event => EventAudience.ForUser(@event.UserId));

		ByTypeDescriptors = byType.ToFrozenDictionary();
		ByTopicDescriptors = byTopic.ToFrozenDictionary();
	}

	private static void Add<TEvent>(
		Dictionary<Type, FusionEventDescriptor> byType,
		Dictionary<string, FusionEventDescriptor> byTopic,
		string topic,
		Func<TEvent, object> mapper,
		Func<TEvent, EventAudience> resolveAudience)
	{
		var descriptor = new FusionEventDescriptor(
			topic,
			typeof(TEvent),
			Map: value => mapper((TEvent)value),
			ResolveAudience: value => resolveAudience((TEvent)value));

		byType.Add(typeof(TEvent), descriptor);
		byTopic.Add(topic, descriptor);
	}

	public FusionEventDescriptor Get<TEvent>()
		=> ByTypeDescriptors.TryGetValue(typeof(TEvent), out var descriptor)
			? descriptor
			: throw new InvalidOperationException($"Fusion event '{typeof(TEvent).FullName}' is not registered.");

	public FusionEventDescriptor Get(string topic)
		=> ByTopicDescriptors.TryGetValue(topic, out var descriptor)
			? descriptor
			: throw new InvalidOperationException($"Fusion event topic '{topic}' is not registered.");
}
