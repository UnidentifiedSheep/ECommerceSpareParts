using Main.Modules.Currency.Abstractions;
using Main.Application.Interfaces.Services.Currency;
using MassTransit;

namespace Main.Application.Consumers;

public class CurrencyRatesChangedConsumer(ICurrencyRateSnapshotProvider rates)
	: IConsumer<CurrencyRateChangedEvent>
{
	public Task Consume(ConsumeContext<CurrencyRateChangedEvent> context)
		=> rates.Reload(context.CancellationToken);
}
