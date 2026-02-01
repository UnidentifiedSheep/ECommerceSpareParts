using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IStorageOwnersRepository
{
    Task<IEnumerable<Storage>> GetUserStoragesAsync(Guid userId, int? page = null, int? limit = null, 
        bool track = true, CancellationToken cancellationToken = default);
    
    Task<StorageOwner?> GetStorageOwnerAsync(Guid userId, string storageName, bool track = true, 
        CancellationToken cancellationToken = default);
}