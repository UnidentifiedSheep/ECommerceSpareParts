using Application.Common.Interfaces.Currency;
using Main.Application.Analytics.Interfaces;
using Main.Application.Analytics.Models;
using Main.Entities.Sale;

namespace Main.Application.Analytics.Services;

public sealed class SaleContentAnalyticsCalculator(ICurrencyConverter converter) : ISaleContentAnalyticsCalculator
{
	public async Task<SaleContentAnalyticsAmounts> CalculateAsync(
		SaleContent content,
		int currencyId,
		DateTime saleDate,
		CancellationToken cancellationToken)
	{
		saleDate = saleDate.ToUniversalTime();
		var originalAmount = await converter.ConvertToBaseAsync(content.OriginalPrice * content.Count, currencyId, saleDate, cancellationToken);
		var revenue = await converter.ConvertToBaseAsync(content.TotalSum, currencyId, saleDate, cancellationToken);
		var cost = 0m;
		foreach (var detail in content.Details)
			cost += await converter.ConvertToBaseAsync(
				detail.BuyPrice * detail.Count,
				detail.CurrencyId,
				detail.PurchaseDatetime.ToUniversalTime(),
				cancellationToken);

		return new SaleContentAnalyticsAmounts(originalAmount, revenue, cost);
	}
}
