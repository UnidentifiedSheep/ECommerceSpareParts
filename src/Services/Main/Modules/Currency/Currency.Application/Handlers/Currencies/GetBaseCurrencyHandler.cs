using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Settings;
using Main.Entities.Currency;
using Main.Entities.Currency.Settings;
using Main.Modules.Currency.Application.Dtos;
using Main.Modules.Currency.Application.Interfaces;

namespace Main.Modules.Currency.Application.Handlers.Currencies;

public record GetBaseCurrencyQuery : IQuery<GetBaseCurrencyResult>;

public record GetBaseCurrencyResult(CurrencyDto Currency);

public class GetBaseCurrencyHandler(
	ISettingsService settingsService,
	ICurrencyCacheRepository cacheRepository) : IQueryHandler<GetBaseCurrencyQuery, GetBaseCurrencyResult>
{
	public async Task<GetBaseCurrencyResult> Handle(
		GetBaseCurrencyQuery request,
		CancellationToken cancellationToken)
	{
		var setting = await settingsService.GetOrDefault<CurrencySetting>(cancellationToken);
		var baseCurrencyId = setting.Data.BaseCurrencyId;

		var currency = await cacheRepository.GetCurrency(baseCurrencyId, cancellationToken) ??
			throw new CurrencyNotFoundException(baseCurrencyId);

		return new GetBaseCurrencyResult(currency);
	}
}
