using Core.Entities;

namespace Core.Interfaces.DbRepositories;

public interface IStoragesRepository
{
    Task<Storage?> GetStorageAsync(string name, bool track = true, CancellationToken cancellationToken = default);
    Task<IEnumerable<Storage>> GetStoragesAsync(string? searchTerm, int page, int viewCount, bool track = true, CancellationToken cancellationToken = default);
    Task<bool> StorageExistsAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> StoragesExistsAsync(IEnumerable<string> names, CancellationToken cancellationToken = default);
}