using Application.Common.Interfaces.Repositories;
using Main.Entities.Sale;

namespace Main.Application.Interfaces.Persistence;

public interface ISaleRepository : IRepository<Sale, Guid>
{
	Task<Sale?> GetFullSaleForUpdate(Guid id, CancellationToken cancellationToken = default);

	Task ReplaceAnalyticsAsync(IReadOnlyCollection<Guid> saleIds, IReadOnlyCollection<SaleContentAnalytics> analytics,
		CancellationToken cancellationToken = default);
}
