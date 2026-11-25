using Application.Common.Extensions;
using Application.Common.Interfaces.Events;
using Enums;
using Integrations.Supplier.Models;
using Main.Modules.Product.Abstractions.Events;
using Microsoft.Extensions.Logging;
using Pricing.Abstractions.Events;
using Pricing.Abstractions.Models;
using Pricing.Application.Events;
using Pricing.Application.Extensions;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Application.Models;
using Pricing.Entities.Enums;
using Pricing.Entities.Offers;

namespace Pricing.Application.Services;

public class OfferRefreshService(
	ISupplierOfferConverterService converterService,
	IPriceOfferRepository offerRepository,
	IIntegrationEventScope integrationEventScope,
	IPriceOfferRefreshStateRepository stateRepository,
	ISupplierOfferRequestMarkerService markerService,
	IDomainEventScope domainEventScope,
	ILogger<OfferRefreshService> logger) : IOfferRefreshService
{
	public async Task<IReadOnlyList<PriceOffer>> ApplyExtractedOffersAsync(
		DateTime extractedAt,
		int productId,
		string storageCode,
		IReadOnlyList<SupplierOfferExtractionResult> extracted,
		CancellationToken token = default)
	{
		var successful = extracted.Where(x => x is { IsSuccess: true, Offer: not null }).ToList();

		var events = successful
			.Select(x => new SupplierProductsRequestedEvent
			{
				Supplier = x.Supplier,
				OccurredAt = extractedAt,
				RequestedStorageCode = storageCode,
				Products = [x.Offer!.ToContract()]
			})
			.ToList();

		var supplierPositions = successful
			.GroupBy(x => x.Supplier)
			.ToDictionary(
				x => x.Key,
				IReadOnlyList<SupplierPosition> (x) => x.SelectMany(r => r.Offer!.Positions).ToList());

		var offers = await RefreshOffersAsync(
			extractedAt,
			productId,
			storageCode,
			supplierPositions,
			token);

		integrationEventScope.AddRange(events);

		return offers;
	}

	public Task<IReadOnlyList<PriceOffer>> RefreshOffersAsync(
		int productId,
		string storageCode,
		IReadOnlyDictionary<Supplier, IReadOnlyList<SupplierPosition>> supplierPositions,
		CancellationToken token = default)
		=> RefreshOffersAsync(
			DateTime.UtcNow,
			productId,
			storageCode,
			supplierPositions,
			token);

	public async Task<IReadOnlyList<PriceOffer>> RefreshOffersAsync(
		DateTime dataExtractionTime,
		string storageCode,
		Supplier supplier,
		IReadOnlyDictionary<int, IReadOnlyList<SupplierPosition>> supplierPositions,
		CancellationToken token = default)
	{
		if (supplierPositions.Count == 0)
			return [];

		var source = supplier.ToSource();

		var events = new List<ProductPriceOffersUpdatedEvent>();
		var offers = new List<PriceOffer>();
		var notFoundCurrencies = new HashSet<string>();
		var refreshed = new HashSet<int>();

		var allowed = await UpsertStatesAsync(
			dataExtractionTime,
			storageCode,
			supplierPositions.Select(x => (x.Key, source, x.Value.Count)),
			token);

		foreach (var (productId, positions) in supplierPositions)
		{
			if (!allowed.Contains(new PriceOfferRefreshStateKey(productId, source, storageCode)))
				continue;

			refreshed.Add(productId);
			events.Add(
				new ProductPriceOffersUpdatedEvent
				{
					ProductId = productId, StorageCode = storageCode
				});

			var result = await converterService.ConvertAsync(
				productId,
				storageCode,
				new Dictionary<Supplier, IReadOnlyList<SupplierPosition>>
				{
					[supplier] = positions
				},
				token);

			foreach (var supplierOffer in result)
			{
				offers.AddRange(supplierOffer.Offers);
				notFoundCurrencies.UnionWith(supplierOffer.NotFoundCurrencies);
			}

			await offerRepository.DeleteOffersAsync(
				productId,
				storageCode,
				[source],
				token);
		}

		LogNotFoundCurrencies(notFoundCurrencies);

		if (offers.Count != 0)
			await offerRepository.UpsertOffersAsync(offers, token);

		PublishRefreshed(
			storageCode,
			supplier,
			offers,
			dataExtractionTime);

		await markerService.MarkAsOkAsync(
			refreshed,
			supplier,
			storageCode,
			token);

		integrationEventScope.AddRange(events);

		return offers;
	}

	private async Task<IReadOnlyList<PriceOffer>> RefreshOffersAsync(
		DateTime dataExtractionTime,
		int productId,
		string storageCode,
		IReadOnlyDictionary<Supplier, IReadOnlyList<SupplierPosition>> supplierPositions,
		CancellationToken token = default)
	{
		if (supplierPositions.Count == 0)
			return [];

		var result = await converterService.ConvertAsync(
			productId,
			storageCode,
			supplierPositions,
			token);

		var offers = new List<PriceOffer>();
		var notFoundCurrencies = new HashSet<string>();

		foreach (var supplierOffer in result)
		{
			offers.AddRange(supplierOffer.Offers);
			notFoundCurrencies.UnionWith(supplierOffer.NotFoundCurrencies);
		}

		LogNotFoundCurrencies(notFoundCurrencies);

		var allowed = await UpsertStatesAsync(
			dataExtractionTime,
			storageCode,
			supplierPositions.Select(x => (productId, x.Key.ToSource(), x.Value.Count)),
			token);

		var refreshedSuppliers = supplierPositions.Keys
			.Where(x => allowed.Contains(new PriceOfferRefreshStateKey(productId, x.ToSource(), storageCode)))
			.ToList();

		var sourcesToRemove = refreshedSuppliers.Select(x => x.ToSource()).ToList();

		if (sourcesToRemove.Count == 0)
			return [];

		await offerRepository.DeleteOffersAsync(
			productId,
			storageCode,
			sourcesToRemove,
			token);

		foreach (var supplier in refreshedSuppliers)
			PublishRefreshed(
				storageCode,
				supplier,
				offers,
				dataExtractionTime);

		if (offers.Count == 0)
			return [];

		var allowedSources = sourcesToRemove.ToHashSet();

		var filteredOffers = offers.Where(x => allowedSources.Contains(x.Source)).ToList();

		if (filteredOffers.Count == 0)
			return filteredOffers;

		integrationEventScope.Add(
			new ProductPriceOffersUpdatedEvent
			{
				StorageCode = storageCode, ProductId = productId
			});
		await offerRepository.UpsertOffersAsync(filteredOffers, token);

		return filteredOffers;
	}

	private async Task<IReadOnlySet<PriceOfferRefreshStateKey>> UpsertStatesAsync(
		DateTime dataExtractionTime,
		string storageCode,
		IEnumerable<(int ProductId, PriceOfferSource Source, int OffersCount)> items,
		CancellationToken token)
	{
		var states = items
			.Select(x =>
			{
				var state = PriceOfferRefreshState.Create(
					x.ProductId,
					x.Source,
					storageCode);

				state.OffersUpdated(dataExtractionTime, x.OffersCount);

				return state;
			})
			.ToList();

		return await stateRepository.UpsertStatesAsync(states, token);
	}

	private void PublishRefreshed(
		string storageCode,
		Supplier supplier,
		IReadOnlyCollection<PriceOffer> offers,
		DateTime seenAt)
	{
		var source = supplier.ToSource();

		var snapshots = offers
			.Where(x => x.Source == source && x.OfferForStorage == storageCode)
			.Select(x => new SupplierOfferSnapshot(
				x.ProductId,
				x.CurrencyId,
				x.PurchasePrice,
				x.DeliveryDate,
				x.GuaranteedDeliveryDate,
				x.DeliveryProbability))
			.ToList();

		if (snapshots.Count == 0)
			return;

		domainEventScope.Add(
			new SupplierOffersRefreshedEvent(
				supplier,
				storageCode,
				seenAt,
				snapshots));
	}

	private void LogNotFoundCurrencies(HashSet<string> notFoundCurrencies)
	{
		if (notFoundCurrencies.Count == 0)
			return;

		logger.LogWarning(
			"Unable to find currency for {CurrencyCode}. When tried to get price offers from supplier.",
			string.Join(", ", notFoundCurrencies));
	}
}
