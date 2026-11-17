using Application.Common.Interfaces.Repositories;
using Main.Modules.Sale.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Sale.Application.Services;

public sealed class SaleSnapshots(IReadRepository<Entities.Sale.Sale.Sale, Guid> sales) : ISaleSnapshots
{
	public async Task<SaleSnapshotsPage> GetPageAsync(
		Guid? afterSaleId,
		int take,
		CancellationToken cancellationToken = default)
	{
		var query = sales.Query.AsNoTracking();
		if (afterSaleId is { } lastId)
			query = query.Where(sale => sale.Id.CompareTo(lastId) > 0);

		var page = await query
			.OrderBy(sale => sale.Id)
			.Take(take)
			.Include(sale => sale.Contents)
			.ThenInclude(content => content.Details)
			.AsSplitQuery()
			.ToListAsync(cancellationToken);

		return new SaleSnapshotsPage(
			page.SelectMany(sale => sale.Contents.Select(content => SaleContentSnapshots.From(content, sale)))
				.ToArray(),
			page.Count == 0 ? null : page[^1].Id,
			page.Count);
	}
}
