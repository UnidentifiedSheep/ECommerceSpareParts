using Main.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Repository;
using QueryExtensions = Persistence.Interfaces.IQueryableExtensions;

namespace Main.Persistence.Repositories.Sale;

public class SaleRepository(UnifiedDbContext context, QueryExtensions extensions)
	: LinqRepositoryBase<UnifiedDbContext, Entities.Sale.Sale, Guid>(context, extensions), ISaleRepository
{
	public Task<Entities.Sale.Sale?> GetFullSaleForUpdate(
		Guid id,
		CancellationToken cancellationToken = default)
		=> FullSalesForUpdate().FirstOrDefaultAsync(sale => sale.Id == id, cancellationToken);

	public async Task<IReadOnlyList<Entities.Sale.Sale>> GetFullSalesForUpdate(
		IReadOnlyCollection<Guid> ids,
		CancellationToken cancellationToken = default)
		=> await FullSalesForUpdate()
			.Where(sale => ids.Contains(sale.Id))
			.OrderBy(sale => sale.Id)
			.ToListAsync(cancellationToken);

	private IQueryable<Entities.Sale.Sale> FullSalesForUpdate()
		=> QueryableExtensions.ForUpdate(Context.Set<Entities.Sale.Sale>().AsQueryable())
			.Include(sale => sale.Contents)
			.ThenInclude(content => content.Details)
			.Include(sale => sale.Contents)
			.ThenInclude(content => content.Analytics)
			.AsSplitQuery();
}
