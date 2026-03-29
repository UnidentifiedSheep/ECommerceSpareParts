using Abstractions.Models.Repository;
using Analytics.Entities;

namespace Analytics.Abstractions.Interfaces.DbRepositories;

public interface IPurchaseFactRepository
{
    Task<PurchasesFact?> GetFact(
        QueryOptions<PurchasesFact, string> options,
        CancellationToken cancellationToken = default);
}