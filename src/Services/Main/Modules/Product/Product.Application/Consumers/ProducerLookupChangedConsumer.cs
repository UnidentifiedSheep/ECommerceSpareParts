using Main.Application.Interfaces.Services;
using Main.Modules.Product.Application.Events;
using MassTransit;

namespace Main.Application.Consumers;

public sealed class ProducerLookupChangedConsumer(IProducerLookupService lookup)
	: IConsumer<Batch<ProducerLookupChangedEvent>>
{
	public Task Consume(ConsumeContext<Batch<ProducerLookupChangedEvent>> context)
	{
		lookup.Invalidate();
		return Task.CompletedTask;
	}
}

public sealed class ProducerLookupChangedDefinition : ConsumerDefinition<ProducerLookupChangedConsumer>
{
	protected override void ConfigureConsumer(
		IReceiveEndpointConfigurator endpointConfigurator,
		IConsumerConfigurator<ProducerLookupChangedConsumer> consumerConfigurator,
		IRegistrationContext context)
		=> consumerConfigurator.Options<BatchOptions>(options => options
			.SetMessageLimit(100)
			.SetTimeLimit(TimeSpan.FromSeconds(1))
			.SetConcurrencyLimit(1));
}
