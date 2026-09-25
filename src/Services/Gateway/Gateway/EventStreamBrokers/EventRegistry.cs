using System.Collections.Frozen;
using Contracts;

namespace Gateway.EventStreamBrokers;

public interface IEventRegistry
{
	FusionEventDescriptor Get<TEvent>();
	FusionEventDescriptor Get(string topic);
}

public class EventRegistry : IEventRegistry
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
			extractUserId: @event => @event.UserId);

		_descriptors = dict.ToFrozenDictionary();
		_byTopicDescriptors = byTopic.ToFrozenDictionary();
	}

	private static void Add<TEvent>(
		Dictionary<Type, FusionEventDescriptor> byType,
		Dictionary<string, FusionEventDescriptor> byTopic,
		string topic,
		Func<TEvent, object> mapper,
		Func<TEvent, Guid?> extractUserId)
	{
		var descriptor = new FusionEventDescriptor(
			topic,
			typeof(TEvent),
			value => mapper((TEvent)value),
			value => extractUserId((TEvent)value));

		byType.Add(typeof(TEvent), descriptor);
		byTopic.Add(topic, descriptor);
	}

	public FusionEventDescriptor Get<TEvent>() => _descriptors[typeof(TEvent)];
	public FusionEventDescriptor Get(string topic) => _byTopicDescriptors[topic];
}
