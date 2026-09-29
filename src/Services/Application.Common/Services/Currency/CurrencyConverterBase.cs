using Application.Common.Interfaces.Currency;
using Application.Common.Models.Currency;

namespace Application.Common.Services.Currency;

public abstract class CurrencyConverterBase : ICurrencyConverter
{
	public decimal Convert(
		decimal value,
		decimal fromRate,
		decimal toRate)
	{
		if (fromRate == toRate)
			return value;

		var baseValue = value / fromRate;
		return baseValue * toRate;
	}

	public decimal ToBase(decimal value, decimal fromRate) => value / fromRate;

	public decimal FromBase(decimal value, decimal toRate) => value * toRate;

	public abstract Task<decimal> ConvertAsync(
		decimal value,
		int fromCurrencyId,
		int toCurrencyId,
		CancellationToken cancellationToken = default);

	public abstract Task<decimal> ConvertFromBaseAsync(
		decimal value,
		int toCurrencyId,
		CancellationToken cancellationToken = default);

	public abstract Task<decimal> ConvertToBaseAsync(
		decimal value,
		int fromCurrencyId,
		CancellationToken cancellationToken = default);

	public CurrencyRates ChangeBaseCurrency(CurrencyRates data, string newBase)
	{
		if (data.BaseCurrencyCode == newBase)
			return data;

		if (!data.Rates.TryGetValue(newBase, out var newBaseRate))
			throw new ArgumentException($"Валюта с кодом '{newBase}' не найдена.");

		var newRates = new Dictionary<string, decimal>
		{
			[data.BaseCurrencyCode] = 1 / newBaseRate
		};

		foreach (var (currency, rate) in data.Rates)
		{
			if (currency == newBase)
				continue;
			newRates[currency] = rate / newBaseRate;
		}

		return new CurrencyRates(newBase, newRates);
	}
}
