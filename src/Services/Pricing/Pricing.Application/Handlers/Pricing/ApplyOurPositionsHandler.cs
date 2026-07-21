using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Attributes;
using Contracts.Pricing;
using Contracts.Storage;
using MediatR;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Entities.Offers;

namespace Pricing.Application.Handlers.Pricing;

[Diagnostics]
[Transactional, AutoSave]
public record ApplyOurPositionsCommand(
    IEnumerable<StorageContentUpdatedEvent> Events
    ) : ICommand;

public class ApplyOurPositionsHandler(
    IPriceOfferRepository offerRepository,
    IIntegrationEventScope integrationEventScope
    ) : ICommandHandler<ApplyOurPositionsCommand>
{
    public async Task<Unit> Handle(ApplyOurPositionsCommand request, CancellationToken cancellationToken)
    {
        var latestEvents = request.Events
            .GroupBy(x => x.StorageContentId)
            .Select(g => g
                .OrderByDescending(x => x.OccurredAt)
                .First())
            .ToList();

        var events = new List<ProductPriceOffersUpdatedEvent>();
        var offers = new List<PriceOffer>();

        foreach (var x in latestEvents)
        {
            events.Add(new ProductPriceOffersUpdatedEvent
            {
                StorageName = x.StorageName,
                ProductId = x.ProductId,
            });
            
            offers.Add(PriceOffer.CreateForOurWarehouse(
                x.ProductId,
                x.CurrencyId,
                x.StorageName,
                x.BuyPrice,
                x.StorageContentId.ToString(),
                x.AvailableCount,
                1,
                1,
                0,
                x.OccurredAt));
        }
        
        integrationEventScope.AddRange(events);
        await offerRepository.UpsertOffersAsync(offers, cancellationToken);
        return Unit.Value;
    }
}