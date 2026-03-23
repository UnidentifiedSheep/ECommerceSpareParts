using Abstractions.Models.Repository;
using Analytics.Entities;

namespace Analytics.Abstractions.Interfaces.DbRepositories;

public interface IPurchaseFactRepository
{
    Task<PurchasesFact?> GetFact(
        string id,
        QueryOptions? options = null,
        CancellationToken cancellationToken = default);
}