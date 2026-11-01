using Main.Application.Interfaces.Persistence;
using Main.Persistence.Context;
using Main.Entities.Sale;
using Microsoft.EntityFrameworkCore;
using Persistence.Repository;
using QueryExtensions = Persistence.Interfaces.IQueryableExtensions;

namespace Main.Persistence.Repositories.Sale;

public class SaleRepository(DContext context, QueryExtensions extensions)
	: LinqRepositoryBase<DContext, Entities.Sale.Sale, Guid>(context, extensions), ISaleRepository
{
	public Task<Entities.Sale.Sale?> GetFullSaleForUpdate(
		Guid id,
		CancellationToken cancellationToken = default)
	{
		return QueryableExtensions
			.ForUpdate(Context.Sales.AsQueryable())
			.Include(x => x.Contents)
			.ThenInclude(x => x.Details)
			.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
	}

	public async Task ReplaceAnalyticsAsync(IReadOnlyCollection<Guid> saleIds, IReadOnlyCollection<SaleContentAnalytics> analytics,
		CancellationToken cancellationToken = default)
	{
		var existing = Context.SaleContentAnalytics.Where(item => saleIds.Contains(item.Content.SaleId));
		var tracked = Context.ChangeTracker.Entries<SaleContentAnalytics>().ToArray();
		if (tracked.Length > 0)
		{
			// A direct DELETE bypasses tracking; detach only facts being replaced to allow reusing their keys.
			var trackedIds = tracked.Select(entry => entry.Entity.SaleContentId).ToArray();
			var replacedIds = await existing.Where(item => trackedIds.Contains(item.SaleContentId))
				.Select(item => item.SaleContentId).ToHashSetAsync(cancellationToken);
			foreach (var entry in tracked.Where(entry => replacedIds.Contains(entry.Entity.SaleContentId)))
				entry.State = EntityState.Detached;
		}

		await existing.ExecuteDeleteAsync(cancellationToken);
		await Context.AddRangeAsync(analytics, cancellationToken);
	}
}
