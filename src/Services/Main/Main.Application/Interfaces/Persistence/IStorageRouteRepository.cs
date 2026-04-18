using Application.Common.Interfaces.Repositories;
using Main.Entities.Storage;

namespace Main.Application.Interfaces.Repositories;

public interface IStorageRouteRepository : IRepository<StorageRoute, Guid>
{
    Task<StorageRoute?> GetActiveRouteAsync(
        string from, 
        string to, 
        Criteria<StorageRoute>? criteria = null, 
        CancellationToken ct = default);

    Task<bool> IsAnyRouteActiveAsync(
        string from,
        string to,
        CancellationToken ct = default);
}