using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IUserPermissionRepository
{
    Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userId, bool track = true, CancellationToken cancellationToken = default);
}