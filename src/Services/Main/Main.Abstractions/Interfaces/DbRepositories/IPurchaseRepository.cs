using Abstractions.Models.Repository;
using Main.Abstractions.Dtos.RepositoryOptionsData;
using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IPurchaseRepository
{
    Task<Purchase?> GetPurchase(
        QueryOptions<Purchase, string> options,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PurchaseContent>> GetPurchaseContent(
        QueryOptions<PurchaseContent, string> options,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Purchase>> GetPurchases(
        QueryOptions<Purchase, GetPurchaseOptionsData> options,
        CancellationToken cancellationToken = default);
}