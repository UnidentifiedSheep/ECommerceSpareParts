using Main.Application.Interfaces.Persistence;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Repository;
using QueryExtensions = Persistence.Interfaces.IQueryableExtensions;

namespace Main.Persistence.Repositories.Sale;

public class SaleRepository(DContext context, QueryExtensions extensions)
	: LinqRepositoryBase<DContext, Entities.Sale.Sale, Guid>(context, extensions), ISaleRepository
{
	public Task<Entities.Sale.Sale?> GetFullSaleForUpdate(Guid id, CancellationToken cancellationToken = default)
		=> FullSalesForUpdate().FirstOrDefaultAsync(sale => sale.Id == id, cancellationToken);

	public async Task<IReadOnlyList<Entities.Sale.Sale>> GetFullSalesForUpdate(
		IReadOnlyCollection<Guid> ids,
		CancellationToken cancellationToken = default)
		=> await FullSalesForUpdate()
			.Where(sale => ids.Contains(sale.Id))
			.OrderBy(sale => sale.Id)
			.ToListAsync(cancellationToken);

	private IQueryable<Entities.Sale.Sale> FullSalesForUpdate() =>
		QueryableExtensions.ForUpdate(Context.Sales.AsQueryable())
			.Include(sale => sale.Contents).ThenInclude(content => content.Details)
			.Include(sale => sale.Contents).ThenInclude(content => content.Analytics)
			.AsSplitQuery();
}
