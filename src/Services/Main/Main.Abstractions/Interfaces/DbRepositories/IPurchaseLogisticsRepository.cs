using System.Linq.Expressions;
using Abstractions.Models.Repository;
using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IPurchaseLogisticsRepository
{
    Task<IEnumerable<PurchaseLogistic>> GetPurchaseLogistics(IEnumerable<string> ids, bool track = true,
        CancellationToken token = default);
    
    Task<PurchaseLogistic?> GetPurchaseLogistics(string id, QueryOptions? config = null, CancellationToken token = default);
}