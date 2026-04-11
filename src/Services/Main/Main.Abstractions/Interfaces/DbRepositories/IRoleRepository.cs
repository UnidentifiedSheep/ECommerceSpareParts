using Main.Entities;
using Main.Entities.Auth;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IRoleRepository
{
    Task<Role?> GetRoleAsync(string name, bool track = true, CancellationToken cancellationToken = default);

    Task<IEnumerable<Role>> GetRolesAsync(
        IEnumerable<string> names,
        bool track = true,
        CancellationToken cancellationToken = default);

    Task<bool> RoleExistsAsync(string name, CancellationToken cancellationToken = default);

    Task<IEnumerable<Role>> SearchRoles(
        string? searchTerm,
        int page,
        int limit,
        bool track = true,
        CancellationToken cancellationToken = default);
}