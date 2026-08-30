using Analytics.Entities;
using Contracts.Sale;

namespace Analytics.Application.Interfaces.Services.FactSynchronizers;

public interface ISaleFactSynchronizer
{
    Task<SalesFact?> SynchronizeAsync(
        SaleUpdatedEvent saleUpdatedEvent,
        CancellationToken cancellationToken = default);

    Task<SalesFact?> SynchronizeAsync(
        SaleDeletedEvent saleDeletedEvent,
        CancellationToken cancellationToken = default);
}
