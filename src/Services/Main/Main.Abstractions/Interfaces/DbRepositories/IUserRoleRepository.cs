using Abstractions.Models.Repository;
using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IUserRoleRepository
{
    Task<IReadOnlyList<UserRole>> GetUserRolesAsync(
        Guid userId,
        PageableQueryOptions<UserRole>? options = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
}