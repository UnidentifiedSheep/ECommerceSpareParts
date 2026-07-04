using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Users;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Auth;
using Main.Entities.User.ValueObjects;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;
using Persistence.Repository;

namespace Main.Persistence.Repositories.User;

public class UserRepository(DContext context, IQueryableExtensions extensions)
    : LinqRepositoryBase<DContext, Entities.User.User, Guid>(context, extensions), IUserRepository
{
    public async Task<UserRolesAndPermissions?> GetUserRolesAndPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var userExists = await Context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == userId, cancellationToken);

        if (!userExists) return null;

        var roles = await Context.UserRoles
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .Select(r => r.RoleName)
            .ToListAsync(cancellationToken);

        var directPermissions = Context.UserPermissions
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => p.Permission);

        var rolePermissions =
            from userRole in Context.UserRoles.AsNoTracking()
            join rolePermission in Context.Set<RolePermission>().AsNoTracking()
                on userRole.RoleName equals rolePermission.RoleName
            where userRole.UserId == userId
            select rolePermission.PermissionName;

        var permissions = await directPermissions
            .Union(rolePermissions)
            .ToListAsync(cancellationToken);

        return new UserRolesAndPermissions
        {
            Roles = roles,
            Permissions = permissions
        };
    }

    public async Task<decimal?> GetUsersDiscountAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await Context.UserDiscounts.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.Discount)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Entities.User.User?> GetUserByPrimaryEmailAsync(
        string email,
        Criteria<Entities.User.User>? criteria = null,
        CancellationToken cancellationToken = default)
    {
        Email norm = email;
        var query =
            from user in Context.Users
            join userEmail in Context.UserEmails on user.Id equals userEmail.UserId
            where userEmail.Email == norm && userEmail.IsPrimary
            select user;

        if (criteria != null) query = QueryableExtensions.Apply(query, criteria);

        return query.FirstOrDefaultAsync(cancellationToken);
    }
}