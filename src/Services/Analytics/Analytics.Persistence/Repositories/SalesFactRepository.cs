using Abstractions.Interfaces.Services;
using Analytics.Application.Interfaces.Repositories;
using Analytics.Entities;
using Analytics.Persistence.Context;
using Application.Common.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Extensions;
using Persistence.Interfaces;
using Persistence.Repository;

namespace Analytics.Persistence.Repositories;

public class SalesFactRepository(DContext context, IQueryableExtensions extensions)
    : RepositoryBase<DContext, SalesFact, Guid>(context, extensions), ISalesFactRepository
{
    public override Task<Dictionary<Guid, SalesFact>> FindByIdsAsync(
        IEnumerable<Guid> ids,
        Criteria<SalesFact>? criteria = null,
        CancellationToken ct = default)
    {
        
        return QueryableExtensions.Apply(Context.SalesFacts, criteria)
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
    }
}