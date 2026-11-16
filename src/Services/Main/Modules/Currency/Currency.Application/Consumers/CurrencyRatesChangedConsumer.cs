using Main.Modules.Currency.Abstractions.Events;
using Main.Modules.Currency.Abstractions.Interfaces;
using MassTransit;

namespace Main.Modules.Currency.Application.Consumers;

public class CurrencyRatesChangedConsumer(ICurrencyRateSnapshotProvider rates)
	: IConsumer<CurrencyRateChangedEvent>
{
	public Task Consume(ConsumeContext<CurrencyRateChangedEvent> context)
		=> rates.Apply(context.Message.Changes, context.CancellationToken);
}
