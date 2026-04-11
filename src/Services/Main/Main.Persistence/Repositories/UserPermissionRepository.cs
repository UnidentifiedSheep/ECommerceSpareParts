using Abstractions.Models.Repository;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Entities.Auth;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class UserPermissionRepository(DContext context) : IUserPermissionRepository
{
    public async Task<IEnumerable<UserPermission>> GetUserPermissionsAsync(
        QueryOptions<UserPermission, Guid> options,
        CancellationToken cancellationToken = default)
    {
        return await context.UserPermissions
            .ApplyOptions(options)
            .Where(x => x.UserId == options.Data)
            .ApplyPaging(options)
            .ToListAsync(cancellationToken);
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