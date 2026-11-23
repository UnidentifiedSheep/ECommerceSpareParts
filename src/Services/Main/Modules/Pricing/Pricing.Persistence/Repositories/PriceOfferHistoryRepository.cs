using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Repository;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Entities.Enums;
using Pricing.Entities.Offers;
using IQueryableExtensions = Persistence.Interfaces.IQueryableExtensions;

namespace Pricing.Persistence.Repositories;

public class PriceOfferHistoryRepository(UnifiedDbContext context, IQueryableExtensions extensions)
	: LinqRepositoryBase<UnifiedDbContext, PriceOfferHistory, long>(context, extensions),
		IPriceOfferHistoryRepository
{
	public async Task<IReadOnlyList<PriceOfferHistory>> GetOpenAsync(
		string storageCode,
		PriceOfferSource source,
		IEnumerable<int> productIds,
		CancellationToken cancellationToken = default)
	{
		var ids = productIds.ToList();
		return await Context
			.Set<PriceOfferHistory>()
			.AsNoTracking()
			.Where(x => x.IsOpen)
			.Where(x => x.Source == source && x.ForStorage == storageCode)
			.Where(x => ids.Contains(x.ProductId))
			.ToListAsync(cancellationToken);
	}

	public async Task AddAsync(
		IEnumerable<PriceOfferHistory> histories,
		CancellationToken cancellationToken = default)
	{
		var items = histories.ToList();
		if (items.Count == 0)
			return;

		await Context.BulkInsertAsync(items, cancellationToken: cancellationToken);
	}

	public async Task MarkSeenAsync(
		IEnumerable<long> ids,
		DateTime seenAt,
		CancellationToken cancellationToken = default)
	{
		var keys = ids.ToList();
		if (keys.Count == 0)
			return;

		await Context
			.Set<PriceOfferHistory>()
			.Where(x => keys.Contains(x.Id))
			.ExecuteUpdateAsync(
				setters => setters.SetProperty(x => x.LastSeenAt, seenAt),
				cancellationToken);
	}

	public async Task CloseAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default)
	{
		var keys = ids.ToList();
		if (keys.Count == 0)
			return;

		await Context
			.Set<PriceOfferHistory>()
			.Where(x => keys.Contains(x.Id))
			.ExecuteUpdateAsync(
				setters => setters.SetProperty(x => x.IsOpen, false),
				cancellationToken);
	}
}
