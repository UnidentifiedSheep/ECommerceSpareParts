using Contracts.Producer;
using Main.Application.Interfaces.Services;
using MassTransit;

namespace Main.Application.Consumers;

public sealed class ProducerLookupChangedConsumer(IProducerLookupService lookup) : IConsumer<ProducerLookupChangedEvent>
{
	public Task Consume(ConsumeContext<ProducerLookupChangedEvent> context)
	{
		lookup.Invalidate();
		return Task.CompletedTask;
	}
}
