using Abstractions.Models.Repository;
using Main.Entities;
using Main.Entities.Auth;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IUserRoleRepository
{
    Task<IReadOnlyList<UserRole>> GetUserRolesAsync(
        QueryOptions<UserRole, Guid> options,
        CancellationToken cancellationToken = default);
}