using System.Linq.Expressions;
using Main.Entities;
using Main.Entities.Purchase;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IPurchaseContentLogisticsRepository
{
    Task<IEnumerable<PurchaseContentLogistic>> GetPurchaseContentLogistics(
        IEnumerable<int> ids,
        bool track = true,
        CancellationToken token = default,
        params Expression<Func<PurchaseContentLogistic, object>>[] includes);
}