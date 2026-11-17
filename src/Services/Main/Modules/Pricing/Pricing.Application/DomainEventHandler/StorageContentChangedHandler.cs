using System.Globalization;
using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Main.Modules.Storage.Abstractions.Events;
using Pricing.Application.Events;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Entities.Offers;

namespace Pricing.Application.DomainEventHandler;

public sealed class StorageContentChangedHandler(
	IPriceOfferRepository offerRepository,
	IIntegrationEventScope integrationEventScope) : BatchableDomainEventHandler<StorageContentChanged>
{
	public override async Task Handle(
		Batch<StorageContentChanged> notification,
		CancellationToken cancellationToken)
	{
		var latest = notification
			.Items
			.GroupBy(x => x.Content.StorageContentId)
			.Select(group => group.OrderByDescending(x => x.OccurredAt).First())
			.ToList();

		var offers = latest
			.Select(x => PriceOffer.CreateForOurWarehouse(
				x.Content.ProductId,
				x.Content.CurrencyId,
				x.Content.StorageCode,
				x.Content.BuyPrice,
				x.Content.StorageContentId.ToString(CultureInfo.InvariantCulture),
				x.Content.AvailableCount,
				1,
				1,
				0,
				x.OccurredAt))
			.ToList();

		integrationEventScope.AddRange(
			latest.Select(x => new ProductPriceOffersUpdatedEvent
			{
				StorageCode = x.Content.StorageCode, ProductId = x.Content.ProductId
			}));

		await offerRepository.UpsertOffersAsync(offers, cancellationToken);
	}
}
