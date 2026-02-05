using System.Linq.Expressions;
using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IPurchaseLogisticsRepository
{
    Task<IEnumerable<PurchaseLogistic>> GetPurchaseLogistics(IEnumerable<string> ids, bool track = true,
        CancellationToken token = default);
    
    Task<PurchaseLogistic?> GetPurchaseLogistics(string id, bool track = true, CancellationToken token = default,
        params Expression<Func<PurchaseLogistic, object?>>[] includes);
}