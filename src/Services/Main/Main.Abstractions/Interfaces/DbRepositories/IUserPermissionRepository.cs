using Abstractions.Models.Repository;
using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IUserPermissionRepository
{
    Task<IEnumerable<Permission>> GetUserPermissionsAsync(
        Guid userId,
        QueryOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetUserPermissionNamesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}