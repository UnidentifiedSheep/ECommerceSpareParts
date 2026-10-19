using MassTransit;

namespace Tests.Stubs;

public sealed class PublishEndpointAccessor
{
	public IPublishEndpoint Publisher { get; set; } = new MessageBrokerStub();
}
