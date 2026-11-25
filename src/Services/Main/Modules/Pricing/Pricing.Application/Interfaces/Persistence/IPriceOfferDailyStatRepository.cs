using Pricing.Entities.Offers;

namespace Pricing.Application.Interfaces.Persistence;

public interface IPriceOfferDailyStatRepository
{
	Task UpsertAsync(
		IReadOnlyCollection<PriceOfferDailyStat> stats,
		CancellationToken cancellationToken = default);
}
