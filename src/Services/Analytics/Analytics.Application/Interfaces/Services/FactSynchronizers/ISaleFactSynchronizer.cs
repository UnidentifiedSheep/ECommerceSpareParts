using Analytics.Entities;
using Contracts.Sale;

namespace Analytics.Application.Interfaces.Services.FactSynchronizers;

public interface ISaleFactSynchronizer : IFactSynchronizer<SalesFact, Guid>
{
    Task<SalesFact?> SynchronizeAsync(
        SaleUpdatedEvent saleUpdatedEvent,
        CancellationToken cancellationToken = default);
}