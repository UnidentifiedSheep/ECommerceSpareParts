using Abstractions.Models.Repository;
using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IPurchaseLogisticsRepository
{
    Task<IEnumerable<PurchaseLogistic>> GetPurchaseLogistics(
        QueryOptions<PurchaseLogistic, IReadOnlyList<string>> options,
        CancellationToken token = default);

    Task<PurchaseLogistic?> GetPurchaseLogistics(
        QueryOptions<PurchaseLogistic, string> options,
        CancellationToken token = default);
}