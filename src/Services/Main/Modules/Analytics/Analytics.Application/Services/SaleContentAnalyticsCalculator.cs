using Main.Modules.Analytics.Application.Models;
using Main.Modules.Currency.Abstractions.Interfaces;
using Main.Modules.Sale.Abstractions;

namespace Main.Modules.Analytics.Application.Services;

public sealed class SaleContentAnalyticsCalculator(ICurrencyConverter converter)
{
	public async Task<SaleContentAnalyticsAmounts> CalculateAsync(
		SaleContentSnapshot content,
		CancellationToken cancellationToken)
	{
		var saleDate = content.SaleDatetime.ToUniversalTime();
		var originalAmount = await converter.ConvertToBaseAsync(
			content.OriginalPrice * content.Count,
			content.CurrencyId,
			saleDate,
			cancellationToken);
		var revenue = await converter.ConvertToBaseAsync(
			content.TotalSum,
			content.CurrencyId,
			saleDate,
			cancellationToken);
		var cost = 0m;
		foreach (var detail in content.Details)
			cost += await converter.ConvertToBaseAsync(
				detail.BuyPrice * detail.Count,
				detail.CurrencyId,
				detail.PurchaseDatetime.ToUniversalTime(),
				cancellationToken);

		return new SaleContentAnalyticsAmounts(
			originalAmount,
			revenue,
			cost);
	}
}
