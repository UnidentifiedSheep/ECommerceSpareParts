using Application.Common.Interfaces.Repositories;
using Pricing.Entities;

namespace Pricing.Application.Interfaces.Persistence;

public interface IPriceOfferRepository : IRepository<PriceOffer, Guid>
{
    Task UpsertOffersAsync(
        IEnumerable<PriceOffer> offers,
        CancellationToken cancellationToken = default);
}