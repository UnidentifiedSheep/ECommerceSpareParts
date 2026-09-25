namespace Gateway.EventStreamBrokers;

public record FusionEventDescriptor(
	string Topic,
	Type EventType,
	Func<object, object> Map);
