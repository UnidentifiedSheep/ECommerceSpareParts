using Analytics.Entities;
using Application.Common.Interfaces.Repositories;

namespace Analytics.Application.Interfaces.Repositories;

public interface ISaleFactRepository : IRepository<SalesFact, Guid>
{
    Task<SalesFact?> GetFullSalesFact(Guid id, CancellationToken cancellationToken = default);
}