using Contracts.Currency;
using Main.Application.Interfaces.Cache;
using Main.Application.Interfaces.Services.Currency;
using MassTransit;

namespace Main.Application.Consumers;

public class CurrencyRatesChangedConsumer(
	ICurrencyCacheRepository currencyCacheRepository,
	IHistoricalRateProvider historicalRateProvider
	) : IConsumer<CurrencyRateChangedEvent>
{
	public async Task Consume(ConsumeContext<CurrencyRateChangedEvent> context)
	{
		foreach (var (id, _) in context.Message.Rates)
			await currencyCacheRepository.InvalidateCurrencyRate(id);

		await historicalRateProvider.Reload(context.CancellationToken);
	}
}
