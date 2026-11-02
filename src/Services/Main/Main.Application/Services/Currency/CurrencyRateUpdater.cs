using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Currency;
using Integrations.ExchangeRate.Interfaces;
using Main.Application.Interfaces.Services.Currency;
using Main.Application.Models.Currency;
using Main.Entities.Currency;
using Main.Entities.Exceptions;
using Main.Entities.Settings;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Services.Currency;

public class CurrencyRateUpdater(
	IExchangeRateClientFactory clientFactory,
	IRepository<Entities.Currency.Currency, int> currencyRepository,
	IReadRepository<CurrencyRate, int> rateRepository,
	ICurrencyConverter converter,
	IUnitOfWork unitOfWork) : ICurrencyRateUpdater
{
	public async Task<UpdateRatesResult> UpdateAsync(CurrencySetting setting, CancellationToken ct)
	{
		var baseCurrency = await currencyRepository.GetById(setting.Data.BaseCurrencyId, ct) ??
			throw new CurrencyNotFoundException(setting.Data.BaseCurrencyId);
		var response = await clientFactory.GetClient(setting.Data.RateProvider).GetRates(ct);
		if (!response.Success || response.Value is null)
			throw new InvalidOperationException("Unable to get exchange rates");

		var source = response.ValueOrThrow;
		var normalized = converter.ChangeBaseCurrency(new CurrencyRates(source.Base, source.Rates), baseCurrency.Code);

		// Serialize updates for this base currency before comparing with the latest persisted rates.
		await currencyRepository.FirstOrDefaultAsync(Criteria<Entities.Currency.Currency>.New()
			.Where(currency => currency.Id == baseCurrency.Id).ForUpdate().Build(), ct);
		var current = await rateRepository.Query.Where(rate => rate.ToCurrencyId == baseCurrency.Id)
			.GroupBy(rate => rate.FromCurrencyId)
			.Select(group => new
			{
				CurrencyId = group.Key,
				Rate = group.OrderByDescending(rate => rate.CreatedAt).ThenByDescending(rate => rate.Id)
					.Select(rate => rate.Rate).First()
			}).ToDictionaryAsync(rate => rate.CurrencyId, rate => rate.Rate, ct);

		var currencies = await currencyRepository.ListAsync(ct: ct);
		var changed = new Dictionary<int, decimal>();
		var notFound = new List<string>();
		var additions = new List<CurrencyRate>();
		foreach (var currency in currencies)
		{
			if (currency.Id == baseCurrency.Id) continue;
			if (!normalized.Rates.TryGetValue(currency.Code, out var value))
			{
				notFound.Add(currency.Code);
				continue;
			}
			if (current.TryGetValue(currency.Id, out var previous) && previous == value) continue;
			additions.Add(CurrencyRate.Create(currency.Id, baseCurrency.Id, value));
			changed[currency.Id] = value;
		}
		if (additions.Count > 0) await unitOfWork.AddRangeAsync(additions, ct);
		return new UpdateRatesResult(changed, notFound);
	}
}
