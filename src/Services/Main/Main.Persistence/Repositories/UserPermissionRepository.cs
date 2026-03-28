using Abstractions.Models.Repository;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class UserPermissionRepository(DContext context) : IUserPermissionRepository
{
    public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(
        Guid userId,
        QueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var permission = await context.UserPermissions
            .ApplyOptions(options)
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);
        return permission.Select(x => x.PermissionNavigation);
    }

    public async Task<IReadOnlyList<string>> GetUserPermissionNamesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await context.UserPermissions
            .Where(x => x.UserId == userId)
            .Select(x => x.Permission)
            .ToListAsync(cancellationToken);
    }
}