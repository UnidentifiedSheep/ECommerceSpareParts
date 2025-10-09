using Core.Entities;

namespace Core.Interfaces.DbRepositories;

public interface IUserRoleRepository
{
    Task<IEnumerable<UserRole>> GetUserRolesAsync(Guid userId, bool track = true, int? limit = null, int? offset = null,
        CancellationToken cancellationToken = default);
    Task<UserRole?> GetUserRoleAsync(Guid userId, Guid roleId, bool track = true, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
}