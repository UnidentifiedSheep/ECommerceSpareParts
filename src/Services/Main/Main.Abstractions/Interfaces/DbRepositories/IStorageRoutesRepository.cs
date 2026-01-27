using System.Linq.Expressions;
using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IStorageRoutesRepository
{
    Task<StorageRoute?> GetStorageRouteAsync(string storageFrom, string storageTo, bool isActive, bool track = true,
        CancellationToken cancellationToken = default, params Expression<Func<StorageRoute,object>>[] includes);

    Task<StorageRoute?> GetStorageRouteAsync(Guid id, bool track = true, CancellationToken cancellationToken = default,
        params Expression<Func<StorageRoute,object>>[] includes);

    Task<IEnumerable<StorageRoute>> GetStorageRoutesAsync(string? storageFrom, string? storageTo,
        bool? isActive, int? page = null, int? limit = null, bool track = true,
        CancellationToken cancellationToken = default, params Expression<Func<StorageRoute,object>>[] includes);
    
    Task<bool> IsAnyActive(string storageFrom, string storageTo, CancellationToken cancellationToken = default);
}