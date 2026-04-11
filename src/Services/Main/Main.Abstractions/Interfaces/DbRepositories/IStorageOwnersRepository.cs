using Abstractions.Models.Repository;
using Main.Entities;
using Main.Entities.Storage;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IStorageOwnersRepository
{
    Task<IEnumerable<Storage>> GetUserStoragesAsync(
        Guid userId,
        int? page = null,
        int? limit = null,
        bool track = true,
        CancellationToken cancellationToken = default);

    Task<StorageOwner?> GetStorageOwnerAsync(
        Guid userId,
        string storageName,
        bool track = true,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StorageOwner>> GetStorageOwnersAsync(
        QueryOptions<StorageOwner, string> options,
        CancellationToken cancellationToken = default);
}