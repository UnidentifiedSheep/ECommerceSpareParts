using Main.Application.Interfaces.Persistence;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Repository;
using QueryExtensions = Persistence.Interfaces.IQueryableExtensions;

namespace Main.Persistence.Repositories.Sale;

public class SaleRepository(DContext context, QueryExtensions extensions)
    : LinqRepositoryBase<DContext, Entities.Sale.Sale, Guid>(context, extensions), ISaleRepository
{
    public Task<Entities.Sale.Sale?> GetFullSaleForUpdate(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return QueryableExtensions.ForUpdate(Context.Sales.AsQueryable())
            .Include(x => x.Contents)
            .ThenInclude(x => x.Details)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}