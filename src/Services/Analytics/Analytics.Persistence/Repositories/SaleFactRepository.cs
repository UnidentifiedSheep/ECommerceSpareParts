using Analytics.Application.Interfaces.Repositories;
using Analytics.Entities;
using Analytics.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;
using Persistence.Repository;

namespace Analytics.Persistence.Repositories;

public class SaleFactRepository(
    DContext context,
    IQueryableExtensions extensions
) : LinqRepositoryBase<DContext, SalesFact, Guid>(context, extensions), ISaleFactRepository
{
    public Task<SalesFact?> GetFullSalesFact(Guid id, CancellationToken cancellationToken = default)
    {
        return Context.SalesFacts
            .Include(x => x.SaleContents)
            .ThenInclude(x => x.Details)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}