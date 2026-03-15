using System.Linq.Expressions;
using Analytics.Entities;

namespace Analytics.Abstractions.Interfaces.DbRepositories;

public interface ISalesRepository
{
    IAsyncEnumerable<SalesFact> GetFacts(Expression<Func<SalesFact, bool>>? where = null);
}