using Abstractions.Models.Repository;
using Main.Abstractions.Dtos.RepositoryOptionsData;
using Main.Entities;
using Main.Entities.Purchase;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IPurchaseRepository
{
    Task<Purchase?> GetPurchase(
        QueryOptions<Purchase, Guid> options,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PurchaseContent>> GetPurchaseContent(
        QueryOptions<PurchaseContent, Guid> options,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Purchase>> GetPurchases(
        QueryOptions<Purchase, GetPurchaseOptionsData> options,
        CancellationToken cancellationToken = default);
}