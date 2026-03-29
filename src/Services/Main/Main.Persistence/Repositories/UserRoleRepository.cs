using Abstractions.Models.Repository;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class UserRoleRepository(DContext context) : IUserRoleRepository
{
    public async Task<IReadOnlyList<UserRole>> GetUserRolesAsync(
        QueryOptions<UserRole, Guid> options,
        CancellationToken cancellationToken = default)
    {
        return await context.UserRoles
            .ApplyOptions(options)
            .Where(x => x.UserId == options.Data)
            .ApplyPaging(options)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        return await context.UserRoles.AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.RoleId == roleId, cancellationToken);
    }
}