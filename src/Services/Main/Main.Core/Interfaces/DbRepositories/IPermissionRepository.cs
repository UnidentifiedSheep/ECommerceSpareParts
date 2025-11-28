using Main.Core.Entities;

namespace Main.Core.Interfaces.DbRepositories;

public interface IPermissionRepository
{
    Task<IEnumerable<Permission>> GetPermissionsAsync(int page, int limit, bool track = true, 
        CancellationToken cancellationToken = default);
    Task<Permission?> GetPermissionAsync(string name, bool track = true, CancellationToken cancellationToken = default);
}