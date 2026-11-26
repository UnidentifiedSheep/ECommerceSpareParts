using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Interfaces;
using Persistence.Repository;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Entities.Enums;
using Pricing.Entities.Offers;

namespace Pricing.Persistence.Repositories;

public class PriceOfferRefreshStateRepository(UnifiedDbContext context, IQueryableExtensions extensions)
	: LinqRepositoryBase<UnifiedDbContext, PriceOfferRefreshState, PriceOfferRefreshStateKey>(
			context,
			extensions),
		IPriceOfferRefreshStateRepository
{
	public async Task<IReadOnlySet<PriceOfferRefreshStateKey>> UpsertStatesAsync(
		IReadOnlyCollection<PriceOfferRefreshState> states,
		CancellationToken cancellationToken = default)
	{
		if (states.Count == 0)
			return new HashSet<PriceOfferRefreshStateKey>();

		var items = states
			.GroupBy(x => x.GetId())
			.Select(group => group.MaxBy(x => x.LastOffersUpdatedAt)!)
			.ToList();

		var productIds = items.Select(x => x.ProductId).ToArray();
		var sources = items.Select(x => x.Source.ToString()).ToArray();
		var storages = items.Select(x => x.StorageCode).ToArray();
		var updatedAts = items.Select(x => x.LastOffersUpdatedAt).ToArray();
		var counts = items.Select(x => x.LastOffersCount).ToArray();

		var rows = await Context.Database
			.SqlQuery<UpsertedRefreshStateRow>(
				$"""
				INSERT INTO public.price_offer_refresh_states AS s (
				    product_id,
				    source,
				    storage_name,
				    last_offers_updated_at,
				    last_offers_count
				)
				SELECT x.product_id, x.source, x.storage_name, x.updated_at, x.offers_count
				FROM unnest(
				    {productIds}::integer[],
				    {sources}::text[],
				    {storages}::text[],
				    {updatedAts}::timestamptz[],
				    {counts}::integer[]
				) AS x(product_id, source, storage_name, updated_at, offers_count)
				ORDER BY x.product_id, x.source, x.storage_name
				ON CONFLICT (product_id, source, storage_name)
				DO UPDATE SET
				    last_offers_updated_at = EXCLUDED.last_offers_updated_at,
				    last_offers_count = EXCLUDED.last_offers_count
				WHERE s.last_offers_updated_at IS NULL
				   OR s.last_offers_updated_at < EXCLUDED.last_offers_updated_at
				RETURNING s.product_id AS "ProductId", s.source AS "Source", s.storage_name AS "StorageCode"
				""")
			.ToListAsync(cancellationToken);

		return rows
			.Select(x => new PriceOfferRefreshStateKey(
				x.ProductId,
				Enum.Parse<PriceOfferSource>(x.Source),
				x.StorageCode))
			.ToHashSet();
	}

	private sealed class UpsertedRefreshStateRow
	{
		public int ProductId { get; init; }

		public string Source { get; init; } = null!;

		public string StorageCode { get; init; } = null!;
	}
}
