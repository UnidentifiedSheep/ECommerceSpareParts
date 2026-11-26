using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Pricing.Abstractions.Events;
using Pricing.Application.Extensions;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Entities.Offers;

namespace Pricing.Application.DomainEventHandler;

public sealed class TrackPriceOfferDailyStatsHandler(IPriceOfferDailyStatRepository repository)
	: BatchableDomainEventHandler<SupplierOffersRefreshedEvent>
{
	public override async Task Handle(
		Batch<SupplierOffersRefreshedEvent> notification,
		CancellationToken cancellationToken)
	{
		var stats = notification.Items
			.SelectMany(refreshed => refreshed.Offers
				.GroupBy(x => (x.ProductId, x.CurrencyId, x.PurchasePrice))
				.Select(group => PriceOfferDailyStat.Create(
					group.Key.ProductId,
					refreshed.Supplier.ToSource(),
					refreshed.StorageCode,
					group.Key.CurrencyId,
					group.Key.PurchasePrice,
					group.Count(),
					refreshed.SeenAt)))
			.GroupBy(x => x.GetId())
			.Select(group => group.Aggregate(PriceOfferDailyStat.Merge))
			.ToList();

		await repository.UpsertAsync(stats, cancellationToken);
	}
}
