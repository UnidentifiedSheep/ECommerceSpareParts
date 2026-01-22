using System.Linq.Expressions;
using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IUserRoleRepository
{
    Task<IEnumerable<UserRole>> GetUserRolesAsync(Guid userId, bool track = true, int? limit = null,
        int? offset = null, CancellationToken cancellationToken = default,
        params Expression<Func<UserRole, object>>[] includes);

    Task<UserRole?> GetUserRoleAsync(Guid userId, Guid roleId, bool track = true,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
}