using Main.Entities.Logistics.Statistics;
using Main.Modules.Logistics.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Repository;
using IQueryableExtensions = Persistence.Interfaces.IQueryableExtensions;

namespace Main.Modules.Logistics.Persistence.Repositories;

public sealed class DeliveryDailyStatRepository(UnifiedDbContext context, IQueryableExtensions extensions)
	: LinqRepositoryBase<UnifiedDbContext, DeliveryDailyStat, DeliveryDailyStatKey>(context, extensions),
		IDeliveryDailyStatRepository
{
	public async Task UpsertAsync(
		IReadOnlyCollection<DeliveryDailyStat> stats,
		CancellationToken cancellationToken = default)
	{
		if (stats.Count == 0)
			return;

		var days = stats.Select(x => x.Day).ToArray();
		var suppliers = stats.Select(x => x.Supplier.ToString()).ToArray();
		var storages = stats.Select(x => x.StorageCode).ToArray();
		var productIds = stats.Select(x => x.ProductId).ToArray();
		var deliveryDays = stats.Select(x => x.DeliveryDays).ToArray();
		var counts = stats.Select(x => x.OffersCount).ToArray();
		var probabilities = stats.Select(x => x.MaxProbability).ToArray();
		var guaranteed = stats.Select(x => x.MinGuaranteedDays).ToArray();
		var firstSeen = stats.Select(x => x.FirstSeenAt).ToArray();
		var lastSeen = stats.Select(x => x.LastSeenAt).ToArray();

		await Context.Database.ExecuteSqlAsync(
			$"""
			INSERT INTO logistics.delivery_daily_stats AS s (
			    day,
			    supplier,
			    storage_code,
			    product_id,
			    delivery_days,
			    offers_count,
			    max_probability,
			    min_guaranteed_days,
			    first_seen_at,
			    last_seen_at
			)
			SELECT x.day, x.supplier, x.storage_code, x.product_id, x.delivery_days,
			       x.offers_count, x.max_probability, x.min_guaranteed_days, x.first_seen_at, x.last_seen_at
			FROM unnest(
			    {days}::date[],
			    {suppliers}::text[],
			    {storages}::text[],
			    {productIds}::integer[],
			    {deliveryDays}::integer[],
			    {counts}::integer[],
			    {probabilities}::integer[],
			    {guaranteed}::integer[],
			    {firstSeen}::timestamptz[],
			    {lastSeen}::timestamptz[]
			) AS x(day, supplier, storage_code, product_id, delivery_days, offers_count, max_probability,
			       min_guaranteed_days, first_seen_at, last_seen_at)
			ORDER BY x.product_id, x.supplier, x.storage_code, x.day, x.delivery_days
			ON CONFLICT (product_id, supplier, storage_code, day, delivery_days)
			DO UPDATE SET
			    offers_count = s.offers_count + EXCLUDED.offers_count,
			    max_probability = GREATEST(s.max_probability, EXCLUDED.max_probability),
			    min_guaranteed_days = LEAST(s.min_guaranteed_days, EXCLUDED.min_guaranteed_days),
			    first_seen_at = LEAST(s.first_seen_at, EXCLUDED.first_seen_at),
			    last_seen_at = GREATEST(s.last_seen_at, EXCLUDED.last_seen_at);
			""",
			cancellationToken);
	}
}
