using Application.Common.Interfaces.Repositories;
using Pricing.Entities;
using Pricing.Entities.Offers;
using Pricing.Enums;

namespace Pricing.Application.Interfaces.Persistence;

public interface IPriceOfferRepository : IRepository<PriceOffer, Guid>
{
    Task UpsertOffersAsync(
        IEnumerable<PriceOffer> offers,
        CancellationToken cancellationToken = default);

    Task DeleteOffersAsync(
        int productId,
        string storageName,
        IEnumerable<PriceOfferSource> sources,
        CancellationToken cancellationToken = default);
}