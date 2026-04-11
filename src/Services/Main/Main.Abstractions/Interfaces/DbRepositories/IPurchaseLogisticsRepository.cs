using Abstractions.Models.Repository;
using Main.Entities;
using Main.Entities.Purchase;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IPurchaseLogisticsRepository
{
    Task<IEnumerable<PurchaseLogistic>> GetPurchaseLogistics(
        QueryOptions<PurchaseLogistic, IReadOnlyList<Guid>> options,
        CancellationToken token = default);

    Task<PurchaseLogistic?> GetPurchaseLogistics(
        QueryOptions<PurchaseLogistic, Guid> options,
        CancellationToken token = default);
}