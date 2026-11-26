using Application.Common.Interfaces.Repositories;
using Main.Entities.Analytics;
using Main.Modules.Analytics.Abstractions.Models.CoPurchase;
using Main.Modules.Analytics.Application.Models.CoPurchase;
using Main.Modules.Sale.Abstractions.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Analytics.Application.Services;

public interface ICoPurchaseDataReader
{
	Task<IReadOnlyList<int>> GetSoldProductIdsAsync(
		CoPurchaseParameters parameters,
		int afterProductId,
		int take,
		CancellationToken cancellationToken);

	Task<int> GetTotalSalesCountAsync(
		CoPurchaseParameters parameters,
		CancellationToken cancellationToken);

	Task<IReadOnlyList<CoPurchaseSaleProduct>> GetSalesAsync(
		CoPurchaseParameters parameters,
		IReadOnlyCollection<int> productIds,
		CancellationToken cancellationToken);

	Task<IReadOnlyDictionary<int, int>> GetSalesCountsAsync(
		CoPurchaseParameters parameters,
		IReadOnlyCollection<int> productIds,
		CancellationToken cancellationToken);
}

public sealed class CoPurchaseDataReader(
	IReadRepository<SaleContentAnalytics, int> analytics,
	ISaleContentSource contents) : ICoPurchaseDataReader
{
	public async Task<IReadOnlyList<int>> GetSoldProductIdsAsync(
		CoPurchaseParameters parameters,
		int afterProductId,
		int take,
		CancellationToken cancellationToken)
		=> await Rows(parameters)
			.Select(row => row.ProductId)
			.Where(productId => productId > afterProductId)
			.Distinct()
			.OrderBy(productId => productId)
			.Take(take)
			.ToListAsync(cancellationToken);

	public Task<int> GetTotalSalesCountAsync(
		CoPurchaseParameters parameters,
		CancellationToken cancellationToken)
		=> Rows(parameters)
			.Select(row => row.SaleId)
			.Distinct()
			.CountAsync(cancellationToken);

	public async Task<IReadOnlyList<CoPurchaseSaleProduct>> GetSalesAsync(
		CoPurchaseParameters parameters,
		IReadOnlyCollection<int> productIds,
		CancellationToken cancellationToken)
	{
		var ids = productIds.Distinct().ToArray();
		if (ids.Length == 0)
			return [];

		var rows = Rows(parameters);
		var saleIds = rows
			.Where(row => ids.Contains(row.ProductId))
			.Select(row => row.SaleId);

		var result = await rows
			.Where(row => saleIds.Contains(row.SaleId))
			.Select(row => new { row.SaleId, row.ProductId })
			.Distinct()
			.ToListAsync(cancellationToken);

		return result.Select(row => new CoPurchaseSaleProduct(row.SaleId, row.ProductId)).ToList();
	}

	public async Task<IReadOnlyDictionary<int, int>> GetSalesCountsAsync(
		CoPurchaseParameters parameters,
		IReadOnlyCollection<int> productIds,
		CancellationToken cancellationToken)
	{
		var ids = productIds.Distinct().ToArray();
		if (ids.Length == 0)
			return new Dictionary<int, int>();

		return await Rows(parameters)
			.Where(row => ids.Contains(row.ProductId))
			.GroupBy(row => row.ProductId)
			.Select(group => new
			{
				ProductId = group.Key,
				SalesCount = group.Select(row => row.SaleId).Distinct().Count()
			})
			.ToDictionaryAsync(row => row.ProductId, row => row.SalesCount, cancellationToken);
	}

	private IQueryable<SaleProductRow> Rows(CoPurchaseParameters parameters)
	{
		var from = parameters.From;
		var to = parameters.EndExclusive();

		return analytics.Query
			.Where(row => row.IsCompleted && row.SaleDatetime >= from && row.SaleDatetime < to)
			.Join(
				contents.Query,
				row => row.SaleContentId,
				content => content.SaleContentId,
				(row, content) => new SaleProductRow
				{
					SaleId = row.SaleId,
					ProductId = content.ProductId
				});
	}

	private sealed class SaleProductRow
	{
		public Guid SaleId { get; init; }

		public int ProductId { get; init; }
	}
}
