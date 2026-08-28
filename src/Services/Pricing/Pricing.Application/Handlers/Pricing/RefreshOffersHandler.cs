using Application.Common.Interfaces.Cqrs;
using Attributes;
using Enums;
using Integrations.Supplier.Models;
using Internal.Integration.Core.Interfaces.Main;
using Internal.Integration.Core.Models.Main.Product;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Entities;
using Pricing.Entities.Offers;

namespace Pricing.Application.Handlers.Pricing;

[Transactional, AutoSave]
public record RefreshOffersCommand(
    int ProductId,
    string StorageCode,
    bool ForceRefresh = false
) : ICommand<RefreshOffersResult>;

public record RefreshOffersResult(IReadOnlyList<PriceOffer> CreatedOffers);

public class RefreshOffersHandler(
    IOfferRefreshService refreshService
    ) : ICommandHandler<RefreshOffersCommand, RefreshOffersResult>
{
    public async Task<RefreshOffersResult> Handle(RefreshOffersCommand request, CancellationToken cancellationToken)
    {
        var offers = await refreshService
            .RefreshOffersAsync(
                request.ProductId, 
                request.StorageCode,
                cancellationToken);
            
        return new RefreshOffersResult(offers);
    }
}
