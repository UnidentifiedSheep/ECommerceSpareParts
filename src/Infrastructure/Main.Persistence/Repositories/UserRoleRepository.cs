using Main.Core.Entities;
using Main.Core.Interfaces.DbRepositories;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class UserRoleRepository(DContext context) : IUserRoleRepository
{
    public async Task<IEnumerable<UserRole>> GetUserRolesAsync(Guid userId, bool track = true, int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.UserRoles
            .Include(x => x.Role)
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.RoleId)
            .ConfigureTracking(track);

        if (offset != null)
            query = query.Skip(offset.Value);

        if (limit != null)
            query = query.Take(limit.Value);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<UserRole?> GetUserRoleAsync(Guid userId, Guid roleId, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.UserRoles.ConfigureTracking(track)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        return await context.UserRoles.AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.RoleId == roleId, cancellationToken);
    }
}