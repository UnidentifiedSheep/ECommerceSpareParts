using Abstractions.Interfaces.Persistence;
using Application.Common.Extensions;
using Application.Common.Interfaces.Events;
using Contracts.Pricing;
using Contracts.Supplier;
using Enums;
using Integrations.Supplier.Models;
using Microsoft.Extensions.Logging;
using Pricing.Application.Extensions;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Entities;
using Pricing.Entities.Offers;
using Pricing.Enums;

namespace Pricing.Application.Services;

public class OfferRefreshService(
    ISupplierOfferExtractorService extractorService,
    ISupplierOfferConverterService converterService,
    IPriceOfferRepository offerRepository,
    IIntegrationEventScope integrationEventScope,
    IPriceOfferRefreshStateRepository stateRepository,
    ISupplierOfferRequestMarkerService markerService,
    ILogger<OfferRefreshService> logger) : IOfferRefreshService
{
    public async Task<IReadOnlyList<PriceOffer>> RefreshOffersAsync(
        int productId,
        string storageName,
        CancellationToken token = default)
    {
        var extracted = await extractorService.ExtractOffers(
            storageName,
            productId,
            token);

        var now = DateTime.UtcNow;

        var successful = extracted
            .Where(x => x is { IsSuccess: true, Offer: not null })
            .ToList();

        var events = successful
            .Select(x => new SupplierProductsRequestedEvent
            {
                Supplier = x.Supplier,
                OccurredAt = now,
                RequestedStorageFor = storageName,
                Products = [x.Offer!.ToContract()]
            })
            .ToList();

        var supplierPositions = successful
            .GroupBy(x => x.Supplier)
            .ToDictionary(
                x => x.Key,
                IReadOnlyList<SupplierPosition> (x) => x
                    .SelectMany(r => r.Offer!.Positions)
                    .ToList());

        var offers = await RefreshOffersAsync(
            dataExtractionTime: now,
            productId: productId,
            storageName: storageName,
            supplierPositions: supplierPositions,
            token: token);

        integrationEventScope.AddRange(events);

        return offers;
    }

    public Task<IReadOnlyList<PriceOffer>> RefreshOffersAsync(
        int productId,
        string storageName,
        IReadOnlyDictionary<Supplier, IReadOnlyList<SupplierPosition>> supplierPositions,
        CancellationToken token = default)
    {
        return RefreshOffersAsync(
            dataExtractionTime: DateTime.UtcNow,
            productId: productId,
            storageName: storageName,
            supplierPositions: supplierPositions,
            token: token);
    }

    private async Task<IReadOnlyList<PriceOffer>> RefreshOffersAsync(
        DateTime dataExtractionTime,
        int productId,
        string storageName,
        IReadOnlyDictionary<Supplier, IReadOnlyList<SupplierPosition>> supplierPositions,
        CancellationToken token = default)
    {
        if (supplierPositions.Count == 0)
            return [];

        var result = await converterService.ConvertAsync(
            productId,
            storageName,
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

        var sourcesToRemove = new List<PriceOfferSource>();

        foreach (var (supplier, positions) in supplierPositions)
        {
            var source = supplier.ToSource();

            var state = PriceOfferRefreshState.Create(
                productId,
                source,
                storageName);

            state.OffersUpdated(dataExtractionTime, positions.Count);

            var canRefresh = await stateRepository
                .UpsertStateAsync(state, token);

            if (!canRefresh) continue;

            sourcesToRemove.Add(source);
        }

        if (sourcesToRemove.Count == 0)
            return [];

        await offerRepository.DeleteOffersAsync(
            productId,
            storageName,
            sourcesToRemove,
            token);

        if (offers.Count == 0) return [];
        
        var allowedSources = sourcesToRemove.ToHashSet();

        var filteredOffers = offers
            .Where(x => allowedSources.Contains(x.Source))
            .ToList();

        if (filteredOffers.Count == 0) return filteredOffers;
            
        integrationEventScope.Add(new ProductPriceOffersUpdatedEvent
        {
            StorageName = storageName,
            ProductId = productId,
        });
        await offerRepository.UpsertOffersAsync(filteredOffers, token);

        return filteredOffers;
    }

    public async Task<IReadOnlyList<PriceOffer>> RefreshOffersAsync(
        DateTime dataExtractionTime,
        string storageName,
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

        foreach (var (productId, positions) in supplierPositions)
        {
            var state = PriceOfferRefreshState.Create(
                productId,
                source,
                storageName);

            state.OffersUpdated(dataExtractionTime, positions.Count);

            var canRefresh = await stateRepository.UpsertStateAsync(
                state,
                token);

            if (!canRefresh) continue;

            refreshed.Add(productId);
            events.Add(new ProductPriceOffersUpdatedEvent
            {
                ProductId = productId,
                StorageName = storageName,
            });
            
            var result = await converterService.ConvertAsync(
                productId,
                storageName,
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
                storageName,
                [source],
                token);
        }

        LogNotFoundCurrencies(notFoundCurrencies);

        if (offers.Count != 0)
            await offerRepository.UpsertOffersAsync(offers, token);
        
        await markerService.MarkAsOkAsync(
            refreshed, 
            supplier, 
            storageName, 
            token);

        integrationEventScope.AddRange(events);

        return offers;
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