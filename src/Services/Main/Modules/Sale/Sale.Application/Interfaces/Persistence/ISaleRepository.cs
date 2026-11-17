using Application.Common.Interfaces.Repositories;

namespace Main.Modules.Sale.Application.Interfaces.Persistence;

public interface ISaleRepository : IRepository<Entities.Sale.Sale.Sale, Guid>
{
	Task<Entities.Sale.Sale.Sale?> GetFullSaleForUpdate(
		Guid id,
		CancellationToken cancellationToken = default);

	Task<IReadOnlyList<Entities.Sale.Sale.Sale>> GetFullSalesForUpdate(
		IReadOnlyCollection<Guid> ids,
		CancellationToken cancellationToken = default);
}
