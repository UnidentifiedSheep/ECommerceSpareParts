using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IUserStorageRepository
{
    Task<IEnumerable<Storage>> GetUserStoragesAsync(Guid userId, int? page = null, int? limit = null, 
        bool track = true, CancellationToken cancellationToken = default);
}