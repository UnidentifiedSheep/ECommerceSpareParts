using Main.Core.Entities;

namespace Main.Core.Interfaces.DbRepositories;

public interface IRoleRepository
{
    Task<Role?> GetRoleAsync(Guid id, bool track = true, CancellationToken cancellationToken = default);
    Task<Role?> GetRoleAsync(string name, bool track = true, CancellationToken cancellationToken = default);

    Task<IEnumerable<Role>> GetRolesAsync(IEnumerable<string> names, bool track = true,
        CancellationToken cancellationToken = default);

    Task<bool> RoleExistsAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> RoleExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Guid>> RolesExistsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> RolesExistsAsync(IEnumerable<string> roleNames,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Role>> SearchRoles(string? searchTerm, int page, int limit, bool track = true,
        CancellationToken cancellationToken = default);
}