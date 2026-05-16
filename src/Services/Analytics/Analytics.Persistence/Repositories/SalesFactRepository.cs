using Analytics.Application.Interfaces.Repositories;
using Analytics.Entities;
using Analytics.Persistence.Context;
using Application.Common.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Extensions;

namespace Analytics.Persistence.Repositories;

public class SalesFactRepository(DContext context) 
    : RepositoryBase<DContext, SalesFact, Guid>(context), ISalesFactRepository
{
    public override Task<Dictionary<Guid, SalesFact>> FindByIdsAsync(
        IEnumerable<Guid> ids, 
        Criteria<SalesFact>? criteria = null, 
        CancellationToken ct = default)
    {
        return Context.SalesFacts
            .Apply(criteria)
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
    }
}