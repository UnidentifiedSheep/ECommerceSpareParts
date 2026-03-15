using System.Linq.Expressions;
using Analytics.Entities;
using Analytics.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Persistence.Repositories;

public class SalesRepository(DContext context)
{
    public IAsyncEnumerable<SalesFact> GetFacts(Expression<Func<SalesFact, bool>>? where = null)
    {
        IQueryable<SalesFact> query = context.SalesFacts;
        query = where == null ? query : query.Where(where);
        return query.AsAsyncEnumerable();
    }
}