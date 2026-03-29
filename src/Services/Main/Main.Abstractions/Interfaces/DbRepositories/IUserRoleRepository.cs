using Abstractions.Models.Repository;
using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IUserRoleRepository
{
    Task<IReadOnlyList<UserRole>> GetUserRolesAsync(
        QueryOptions<UserRole, Guid> options,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
}