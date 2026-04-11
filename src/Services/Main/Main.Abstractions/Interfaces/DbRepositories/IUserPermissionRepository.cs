using Abstractions.Models.Repository;
using Main.Entities;
using Main.Entities.Auth;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IUserPermissionRepository
{
    Task<IEnumerable<UserPermission>> GetUserPermissionsAsync(
        QueryOptions<UserPermission, Guid> options,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetUserPermissionNamesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}