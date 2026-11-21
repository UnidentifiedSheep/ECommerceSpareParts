using System.Collections.Frozen;
using Abstractions.Interfaces;

namespace Gateway.EventStreamBrokers;

public interface IEventRegistry
{
	FusionEventDescriptor Get<TEvent>();
	FusionEventDescriptor Get(string topic);
}

public abstract class EventRegistryBase : IEventRegistry
{
	private readonly Dictionary<string, FusionEventDescriptor> _byTopic = new();

	private readonly Dictionary<Type, FusionEventDescriptor> _byType = new();

	private bool _registrationCompleted;

	public FrozenDictionary<Type, FusionEventDescriptor> ByTypeDescriptors { get; private set; } =
		FrozenDictionary<Type, FusionEventDescriptor>.Empty;

	public FrozenDictionary<string, FusionEventDescriptor> ByTopicDescriptors { get; private set; } =
		FrozenDictionary<string, FusionEventDescriptor>.Empty;

	public FusionEventDescriptor Get<TEvent>()
		=> ByTypeDescriptors.TryGetValue(typeof(TEvent), out var descriptor)
			? descriptor
			: throw new InvalidOperationException(
				$"Fusion event '{typeof(TEvent).FullName}' is not registered.");

	public FusionEventDescriptor Get(string topic)
		=> ByTopicDescriptors.TryGetValue(topic, out var descriptor)
			? descriptor
			: throw new InvalidOperationException($"Fusion event topic '{topic}' is not registered.");

	protected void CompleteRegistration()
	{
		EnsureRegistrationOpen();
		ByTypeDescriptors = _byType.ToFrozenDictionary();
		ByTopicDescriptors = _byTopic.ToFrozenDictionary();

		_registrationCompleted = true;
		_byTopic.Clear();
		_byType.Clear();
	}

	protected void Add<TEvent>(
		string topic,
		Func<TEvent, object> mapper,
		Func<TEvent, EventAudience> resolveAudience,
		params IServiceDefinition[] routedFrom)
	{
		EnsureRegistrationOpen();
		var descriptor = new FusionEventDescriptor(
			topic,
			typeof(TEvent),
			value => mapper((TEvent)value),
			value => resolveAudience((TEvent)value),
			routedFrom);

		_byType.Add(typeof(TEvent), descriptor);
		_byTopic.Add(topic, descriptor);
	}

	private void EnsureRegistrationOpen()
	{
		if (_registrationCompleted)
			throw new InvalidOperationException("Event registration has already been completed.");
	}
}
