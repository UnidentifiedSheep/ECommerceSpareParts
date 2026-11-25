using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Main.Entities.Logistics.Statistics;
using Main.Modules.Logistics.Abstractions.Interfaces;
using Main.Modules.Logistics.Application.Interfaces.Persistence;
using Pricing.Abstractions.Events;
using Pricing.Abstractions.Models;

namespace Main.Modules.Logistics.Application.DomainEventHandlers.SupplierOffersRefreshed;

public sealed class TrackDeliveryDailyStatsHandler(
	IDeliveryDailyStatRepository repository,
	IDeliveryEstimator deliveryEstimator)
	: BatchableDomainEventHandler<SupplierOffersRefreshedEvent>
{
	public override async Task Handle(
		Batch<SupplierOffersRefreshedEvent> notification,
		CancellationToken cancellationToken)
	{
		var stats = new List<DeliveryDailyStat>();

		foreach (var refreshed in notification.Items)
			stats.AddRange(await CreateStatsAsync(refreshed, cancellationToken));

		var merged = stats
			.GroupBy(x => x.GetId())
			.Select(group => group.Aggregate(DeliveryDailyStat.Merge))
			.ToList();

		await repository.UpsertAsync(merged, cancellationToken);
	}

	private async Task<IEnumerable<DeliveryDailyStat>> CreateStatsAsync(
		SupplierOffersRefreshedEvent refreshed,
		CancellationToken cancellationToken)
	{
		var from = DateOnly.FromDateTime(refreshed.SeenAt);

		var estimates = await deliveryEstimator.EstimateAsync(
			refreshed.Supplier,
			refreshed.StorageCode,
			from,
			refreshed.Offers
				.Select(x => GetSupplierDate(x.DeliveryDate, from))
				.Concat(refreshed.Offers
					.Where(x => x.GuaranteedDeliveryDate.HasValue)
					.Select(x => GetSupplierDate(x.GuaranteedDeliveryDate, from))),
			cancellationToken);

		return refreshed.Offers.Select(x => DeliveryDailyStat.Create(
			refreshed.Supplier,
			refreshed.StorageCode,
			x.ProductId,
			estimates[GetSupplierDate(x.DeliveryDate, from)].DeliveryDays,
			x.DeliveryProbability,
			x.GuaranteedDeliveryDate.HasValue
				? estimates[GetSupplierDate(x.GuaranteedDeliveryDate, from)].DeliveryDays
				: null,
			refreshed.SeenAt));
	}

	private static DateOnly GetSupplierDate(DateTime? date, DateOnly from)
		=> date is { } value ? DateOnly.FromDateTime(value) : from;
}
