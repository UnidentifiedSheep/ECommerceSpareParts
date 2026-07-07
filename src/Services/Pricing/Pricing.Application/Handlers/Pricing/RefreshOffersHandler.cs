using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Attributes;
using Contracts.Supplier;
using Integrations.Supplier.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Entities;

namespace Pricing.Application.Handlers.Pricing;

[Diagnostics(maxExecutionTimeMs: 400)]
[Transactional, AutoSave]
public record RefreshOffersCommand(int ProductId) : ICommand<RefreshOffersResult>;

public record RefreshOffersResult(IReadOnlyList<PriceOffer> CreatedOffers);

public class RefreshOffersHandler(
    ISupplierOfferExtractorService extractorService,
    IIntegrationEventScope integrationEventScope,
    ILogger<RefreshOffersCommand> logger,
    ISupplierOfferConverterService converterService,
    IPriceOfferRepository offerRepository
    ) : ICommandHandler<RefreshOffersCommand, RefreshOffersResult>
{
    public async Task<RefreshOffersResult> Handle(RefreshOffersCommand request, CancellationToken cancellationToken)
    {
        var extracted = await extractorService
            .ExtractOffers(
                request.ProductId, 
                cancellationToken);
        
        var events = extracted
            .Where(x => x is { IsSuccess: true, Offer: not null })
            .Select(x => new SupplierProductsRequestedEvent
            {
                Supplier = x.Supplier,
                Products = [x.Offer!.ToContract()]
            })
            .ToList();

        var toRefresh = extracted
            .Where(x => x is { IsSuccess: true, Offer: not null })
            .GroupBy(x => x.Supplier)
            .ToDictionary(
                x => x.Key, 
                IReadOnlyList<SupplierPosition> (x) => x
                    .SelectMany(r => r.Offer!.Positions)
                    .ToList());
        
        var result = await converterService
            .ConvertAsync(
                request.ProductId, 
                toRefresh, 
                cancellationToken);
        
        var offers = new List<PriceOffer>();
        var notFoundCurrencies = new HashSet<string>();

        foreach (var supplierOffer in result)
        {
            offers.AddRange(supplierOffer.Offers);
            notFoundCurrencies.UnionWith(supplierOffer.NotFoundCurrencies);
        }
        
        LogNotFoundCurrencies(notFoundCurrencies);
        await offerRepository.UpsertOffersAsync(offers, cancellationToken);
        integrationEventScope.AddRange(events);

        return new RefreshOffersResult(offers);
    }

    private void LogNotFoundCurrencies(HashSet<string> notFoundCurrencies)
    {
        if (notFoundCurrencies.Count == 0) return;
        logger.LogWarning("Unable to find currency for {CurrencyCode}. " +
                          "When tried to get price offers from supplier.", 
            string.Join(", ", notFoundCurrencies));
    }
}