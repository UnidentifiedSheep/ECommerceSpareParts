using Main.Core.Entities;

namespace Main.Core.Interfaces.DbRepositories;

public interface IUserPermissionRepository
{
    Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userId, bool track = true, CancellationToken cancellationToken = default);
}