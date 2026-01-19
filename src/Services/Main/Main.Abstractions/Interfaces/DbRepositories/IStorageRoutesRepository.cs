using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IStorageRoutesRepository
{
    Task<StorageRoute?> GetStorageRouteAsync(string storageFrom, string storageTo, bool isActive, bool track = true,
        CancellationToken cancellationToken = default);

    Task<StorageRoute?> GetStorageRouteAsync(Guid id, bool track = true, CancellationToken cancellationToken = default);
}