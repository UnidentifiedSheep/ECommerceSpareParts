using System.Collections.Frozen;
using Contracts;

namespace Gateway.EventStreamBrokers;

public class FusionEventRegistry
{
	private readonly FrozenDictionary<string, FusionEventDescriptor> _descriptors;

	public FusionEventRegistry()
	{
		var dict = new Dictionary<string, FusionEventDescriptor>();

		Add<InAppNotificationCreatedEvent>(
			dict: dict,
			topic: "onNotificationCreated",
			mapper: @event => new
			{
				id = @event.Id
			});

		_descriptors = dict.ToFrozenDictionary();
	}

	private void Add<TEvent>(
		Dictionary<string, FusionEventDescriptor> dict,
		string topic,
		Func<TEvent, object> mapper)
	{
		dict.Add(
			topic,
			new FusionEventDescriptor(
				topic,
				typeof(TEvent),
				value => mapper((TEvent)value)));
	}

	public FusionEventDescriptor Get(string topic) => _descriptors[topic];
}
