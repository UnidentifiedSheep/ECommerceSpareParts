using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Settings;
using Main.Application.Dtos.Currencies;
using Main.Application.Interfaces.Cache;
using Main.Entities.Exceptions;
using Main.Entities.Settings;

namespace Main.Application.Handlers.Currencies;

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
