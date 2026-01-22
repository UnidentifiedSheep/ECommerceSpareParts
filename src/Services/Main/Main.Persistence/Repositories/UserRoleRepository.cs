using System.Linq.Expressions;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class UserRoleRepository(DContext context) : IUserRoleRepository
{
    public async Task<IEnumerable<UserRole>> GetUserRolesAsync(Guid userId, bool track = true, int? limit = null,
        int? offset = null, CancellationToken cancellationToken = default, params Expression<Func<UserRole, object>>[] includes)
    {
        IQueryable<UserRole> query = context.UserRoles;

        foreach (var include in includes)
            query = query.Include(include);
        
        query = query
            .ConfigureTracking(track)
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.RoleId);
        

        if (offset.HasValue)
            query = query.Skip(offset.Value);

        if (limit.HasValue)
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