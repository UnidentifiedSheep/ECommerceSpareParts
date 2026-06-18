using Application.Common.Interfaces.Repositories;
using Main.Entities.Sale;

namespace Main.Application.Interfaces.Persistence;

public interface ISaleRepository : IRepository<Sale, Guid>
{
    public Task<Sale?> GetFullSaleForUpdate(Guid id, CancellationToken cancellationToken = default);
}