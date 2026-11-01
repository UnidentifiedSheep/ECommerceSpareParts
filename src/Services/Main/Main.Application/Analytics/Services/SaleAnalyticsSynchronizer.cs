using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Main.Application.Analytics.Interfaces;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Sale;
using Main.Enums;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Analytics.Services;

public sealed class SaleAnalyticsSynchronizer(
	IReadRepository<SaleContent, int> contents,
	ISaleRepository sales,
	IUnitOfWork unitOfWork,
	ICurrencyConverter converter) : ISaleAnalyticsSynchronizer
{
	public async Task SynchronizeAsync(IReadOnlyCollection<Guid> saleIds, CancellationToken cancellationToken)
	{
		if (saleIds.Count == 0) return;
		if (!unitOfWork.HasActiveTransaction)
			throw new InvalidOperationException("Sale analytics synchronization requires an active transaction.");

		// Lock sales before reading their current financial data, including during LRT rebuilds.
		await sales.ListAsync(
			Criteria<Sale>.New()
				.Where(sale => saleIds.Contains(sale.Id))
				.OrderByAsc(sale => sale.Id)
				.Include(x => x.Contents)
				.ForUpdate()
				.Build(),
			cancellationToken);

		var rows = await contents.Query
			.Where(content => saleIds.Contains(content.SaleId) && content.Sale.State == SaleState.Completed)
			.Select(content => new
			{
				content.Id, content.Sale.CurrencyId, content.Sale.SaleDatetime,
				OriginalAmount = content.OriginalPrice * content.Count, Revenue = content.TotalSum,
				Details = content.Details.Select(detail => new
				{
					detail.CurrencyId, detail.PurchaseDatetime, Amount = detail.BuyPrice * detail.Count
				}).ToList()
			}).ToListAsync(cancellationToken);

		var facts = new List<SaleContentAnalytics>(rows.Count);
		foreach (var row in rows)
		{
			var originalAmount = await converter.ConvertToBaseAsync(row.OriginalAmount, row.CurrencyId, row.SaleDatetime, cancellationToken);
			var revenue = await converter.ConvertToBaseAsync(row.Revenue, row.CurrencyId, row.SaleDatetime, cancellationToken);
			var cost = 0m;
			foreach (var detail in row.Details)
				cost += await converter.ConvertToBaseAsync(detail.Amount, detail.CurrencyId, detail.PurchaseDatetime, cancellationToken);

			facts.Add(SaleContentAnalytics.Create(row.Id, originalAmount, revenue, cost));
		}

		await sales.ReplaceAnalyticsAsync(saleIds, facts, cancellationToken);
	}
}
