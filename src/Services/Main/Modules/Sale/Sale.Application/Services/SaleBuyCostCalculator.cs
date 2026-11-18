using Application.Common.Interfaces.Repositories;
using Main.Entities.Sale.Sale;
using Main.Modules.Currency.Abstractions.Interfaces;
using Main.Modules.Sale.Application.Dtos.Sale;
using Main.Modules.Sale.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Sale.Application.Services;

public sealed class SaleBuyCostCalculator(
	IReadRepository<SaleContentDetail, int> details,
	ICurrencyConverter converter) : ISaleBuyCostCalculator
{
	public async Task<IReadOnlyDictionary<int, decimal>> CalculateAsync(
		IReadOnlyCollection<ProductSaleHistoryDto> sales,
		CancellationToken cancellationToken)
	{
		var saleCurrencies = sales.ToDictionary(x => x.SaleContentId, x => x.CurrencyId);
		var ids = saleCurrencies.Keys.ToList();

		var groups = await details.Query
			.Where(x => ids.Contains(x.SaleContentId))
			.GroupBy(x => new { x.SaleContentId, x.CurrencyId, Day = x.PurchaseDatetime.Date })
			.Select(g => new BuyCostGroup(
				g.Key.SaleContentId,
				g.Key.CurrencyId,
				g.Key.Day,
				g.Sum(x => x.BuyPrice * x.Count)))
			.ToListAsync(cancellationToken);

		var result = ids.ToDictionary(id => id, _ => 0m);
		foreach (var group in groups)
		{
			var target = saleCurrencies[group.SaleContentId];
			result[group.SaleContentId] += group.CurrencyId == target
				? group.Total
				: await converter.ConvertAsync(
					group.Total,
					group.CurrencyId,
					target,
					group.Date,
					cancellationToken);
		}

		return result;
	}

	private sealed record BuyCostGroup(int SaleContentId, int CurrencyId, DateTime Date, decimal Total);
}
