using Application.Common.Interfaces.Repositories;
using Pricing.Entities.Offers;

namespace Pricing.Application.Interfaces.Persistence;

public interface
	IPriceOfferRefreshStateRepository : IRepository<PriceOfferRefreshState, PriceOfferRefreshStateKey>
{
	Task<IReadOnlySet<PriceOfferRefreshStateKey>> UpsertStatesAsync(
		IReadOnlyCollection<PriceOfferRefreshState> states,
		CancellationToken cancellationToken = default);
}
