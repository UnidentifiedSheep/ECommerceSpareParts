using Application.Common.Interfaces.Repositories;
using Pricing.Entities;
using Pricing.Entities.Offers;

namespace Pricing.Application.Interfaces.Persistence;

public interface IPriceOfferRefreshStateRepository : IRepository<PriceOfferRefreshState, PriceOfferRefreshStateKey>
{
    Task<bool> UpsertStateAsync(
        PriceOfferRefreshState model,
        CancellationToken cancellationToken = default);
}