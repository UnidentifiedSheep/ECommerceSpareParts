using Application.Common.Interfaces.Repositories;
using Pricing.Entities.Enums;
using Pricing.Entities.Offers;

namespace Pricing.Application.Interfaces.Persistence;

public interface IPriceOfferHistoryRepository : IRepository<PriceOfferHistory, long>
{
	Task<IReadOnlyList<PriceOfferHistory>> GetOpenAsync(
		string storageCode,
		PriceOfferSource source,
		IEnumerable<int> productIds,
		CancellationToken cancellationToken = default);

	Task AddAsync(IEnumerable<PriceOfferHistory> histories, CancellationToken cancellationToken = default);

	Task MarkSeenAsync(IEnumerable<long> ids, DateTime seenAt, CancellationToken cancellationToken = default);

	Task CloseAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default);
}
