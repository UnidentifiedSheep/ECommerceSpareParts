using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Main.Entities.Logistics.Statistics;
using Main.Modules.Logistics.Application.Interfaces.Persistence;
using Pricing.Abstractions.Events;

namespace Main.Modules.Logistics.Application.DomainEventHandlers.SupplierOffersRefreshed;

public sealed class TrackDeliveryDailyStatsHandler(IDeliveryDailyStatRepository repository)
	: BatchableDomainEventHandler<SupplierOffersRefreshedEvent>
{
	public override async Task Handle(
		Batch<SupplierOffersRefreshedEvent> notification,
		CancellationToken cancellationToken)
	{
		var stats = notification.Items
			.SelectMany(refreshed => refreshed.Offers.Select(x => DeliveryDailyStat.Create(
				refreshed.Supplier,
				refreshed.StorageCode,
				x.ProductId,
				x.DeliveryDays,
				x.DeliveryProbability,
				x.GuaranteedDeliveryDays,
				refreshed.SeenAt)))
			.GroupBy(x => x.GetId())
			.Select(group => group.Aggregate(DeliveryDailyStat.Merge))
			.ToList();

		await repository.UpsertAsync(stats, cancellationToken);
	}
}
