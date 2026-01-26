using System.Linq.Expressions;
using Main.Entities;
using Main.Enums;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IStoragesRepository
{
    Task<Storage?> GetStorageAsync(string name, bool track = true, CancellationToken cancellationToken = default, 
        params Expression<Func<Storage, object>>[] includes);

    Task<IEnumerable<Storage>> GetStoragesAsync(string? searchTerm, int page, int viewCount,
        bool track = true, StorageType? type = null, CancellationToken cancellationToken = default);

    Task<bool> StorageExistsAsync(string name, CancellationToken cancellationToken = default);
}