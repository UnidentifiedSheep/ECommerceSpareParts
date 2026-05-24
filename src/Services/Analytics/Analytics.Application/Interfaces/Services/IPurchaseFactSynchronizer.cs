using Analytics.Entities;

namespace Analytics.Application.Interfaces.Services;

public interface IPurchaseFactSynchronizer
{
    Task<PurchasesFact?> SynchronizeAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}