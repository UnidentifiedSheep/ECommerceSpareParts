using Exceptions.Exceptions.Roles;
using Main.Core.Interfaces.DbRepositories;

namespace Main.Application.Extensions;

public static class RoleRepositoryExtensions
{
    public static async Task EnsureRoleExists(this IRoleRepository repository, Guid roleId,
        CancellationToken cancellationToken = default)
    {
        if (!await repository.RoleExistsAsync(roleId, cancellationToken))
            throw new RoleNotFoundException(roleId);
    }

    public static async Task EnsureRolesExists(this IRoleRepository repository, IEnumerable<Guid> roleIds,
        CancellationToken cancellationToken = default)
    {
        var notFoundRoles = await repository.RolesExistsAsync(roleIds, cancellationToken);
        var list = notFoundRoles.ToList();
        if (list.Count != 0)
            throw new RoleNotFoundException(list);
    }

    public static async Task EnsureRoleExists(this IRoleRepository repository, string roleName,
        CancellationToken cancellationToken = default)
    {
        var exists = await repository.RoleExistsAsync(roleName, cancellationToken);
        if (exists)
            throw new RoleNotFoundException(roleName);
    }

    public static async Task EnsureRolesExists(this IRoleRepository repository, IEnumerable<string> roleNames,
        CancellationToken cancellationToken = default)
    {
        var set = roleNames.ToHashSet();
        var notFound = (await repository.RolesExistsAsync(set, cancellationToken)).ToList();
        if (notFound.Count != 0)
            throw new RoleNotFoundException(notFound);
    }
}