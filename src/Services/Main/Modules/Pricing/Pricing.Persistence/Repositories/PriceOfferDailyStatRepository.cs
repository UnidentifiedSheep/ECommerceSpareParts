using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Interfaces;
using Persistence.Repository;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Entities.Offers;

namespace Pricing.Persistence.Repositories;

public class PriceOfferDailyStatRepository(UnifiedDbContext context, IQueryableExtensions extensions)
	: LinqRepositoryBase<UnifiedDbContext, PriceOfferDailyStat, PriceOfferDailyStatKey>(context, extensions),
		IPriceOfferDailyStatRepository
{
	public async Task UpsertAsync(
		IReadOnlyCollection<PriceOfferDailyStat> stats,
		CancellationToken cancellationToken = default)
	{
		if (stats.Count == 0)
			return;

		var days = stats.Select(x => x.Day).ToArray();
		var productIds = stats.Select(x => x.ProductId).ToArray();
		var sources = stats.Select(x => x.Source.ToString()).ToArray();
		var storages = stats.Select(x => x.ForStorage).ToArray();
		var currencyIds = stats.Select(x => x.CurrencyId).ToArray();
		var prices = stats.Select(x => x.PurchasePrice).ToArray();
		var counts = stats.Select(x => x.OffersCount).ToArray();
		var firstSeen = stats.Select(x => x.FirstSeenAt).ToArray();
		var lastSeen = stats.Select(x => x.LastSeenAt).ToArray();

		await Context.Database.ExecuteSqlAsync(
			$"""
			INSERT INTO public.price_offer_daily_stats AS s (
			    day,
			    product_id,
			    source,
			    for_storage,
			    currency_id,
			    price,
			    offers_count,
			    first_seen_at,
			    last_seen_at
			)
			SELECT x.day, x.product_id, x.source, x.for_storage, x.currency_id, x.price,
			       x.offers_count, x.first_seen_at, x.last_seen_at
			FROM unnest(
			    {days}::date[],
			    {productIds}::integer[],
			    {sources}::text[],
			    {storages}::text[],
			    {currencyIds}::integer[],
			    {prices}::numeric[],
			    {counts}::integer[],
			    {firstSeen}::timestamptz[],
			    {lastSeen}::timestamptz[]
			) AS x(day, product_id, source, for_storage, currency_id, price, offers_count, first_seen_at, last_seen_at)
			ORDER BY x.product_id, x.source, x.for_storage, x.day, x.currency_id, x.price
			ON CONFLICT (product_id, source, for_storage, day, currency_id, price)
			DO UPDATE SET
			    offers_count = s.offers_count + EXCLUDED.offers_count,
			    first_seen_at = LEAST(s.first_seen_at, EXCLUDED.first_seen_at),
			    last_seen_at = GREATEST(s.last_seen_at, EXCLUDED.last_seen_at);
			""",
			cancellationToken);
	}
}
