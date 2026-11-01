using Application.Common.Interfaces.Currency;

namespace Application.Common.Services.Currency;

public class CurrencyConverter(ICurrencyRatesProvider ratesProvider) : CurrencyConverterBase
{
	public override async Task<decimal> ConvertAsync(
		decimal value,
		int fromCurrencyId,
		int toCurrencyId,
		CancellationToken cancellationToken = default)
	{
		var fromRate = await ratesProvider.GetRate(fromCurrencyId, cancellationToken);
		var toRate = await ratesProvider.GetRate(toCurrencyId, cancellationToken);
		return Convert(
			value,
			fromRate,
			toRate);
	}

	public override async Task<decimal> ConvertFromBaseAsync(
		decimal value,
		int toCurrencyId,
		CancellationToken cancellationToken = default)
	{
		var rate = await ratesProvider.GetRate(toCurrencyId, cancellationToken);
		return FromBase(value, rate);
	}

	public override async Task<decimal> ConvertToBaseAsync(
		decimal value,
		int fromCurrencyId,
		CancellationToken cancellationToken = default)
	{
		var rate = await ratesProvider.GetRate(fromCurrencyId, cancellationToken);
		return ToBase(value, rate);
	}
	public override async Task<decimal> ConvertAsync(decimal value, int fromCurrencyId, int toCurrencyId,
		DateTime forDate, CancellationToken cancellationToken = default)
	{
		ValidateDate(forDate);
		if (fromCurrencyId == toCurrencyId) return value;
		var fromRate = await ratesProvider.GetRate(fromCurrencyId, forDate, cancellationToken);
		var toRate = await ratesProvider.GetRate(toCurrencyId, forDate, cancellationToken);
		return Convert(value, fromRate, toRate);
	}

	public override async Task<decimal> ConvertToBaseAsync(decimal value, int fromCurrencyId,
		DateTime forDate, CancellationToken cancellationToken = default)
	{
		ValidateDate(forDate);
		return ToBase(value, await ratesProvider.GetRate(fromCurrencyId, forDate, cancellationToken));
	}

	public override async Task<decimal> ConvertFromBaseAsync(decimal value, int toCurrencyId,
		DateTime forDate, CancellationToken cancellationToken = default)
	{
		ValidateDate(forDate);
		return FromBase(value, await ratesProvider.GetRate(toCurrencyId, forDate, cancellationToken));
	}

	private static void ValidateDate(DateTime forDate)
	{
		if (forDate.Kind != DateTimeKind.Utc)
			throw new ArgumentException("Currency conversion date must be UTC.", nameof(forDate));
	}

}
