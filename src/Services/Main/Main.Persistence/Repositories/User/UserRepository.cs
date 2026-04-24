using Main.Application.Dtos.Users;
using Main.Application.Interfaces.Persistence;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Main.Persistence.Repositories.User;

public class UserRepository(DContext context) : RepositoryBase<DContext, Entities.User.User, Guid>(context), IUserRepository
{
    public Task<UserRolesAndPermissions?> GetUserRolesAndPermissionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return Context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new UserRolesAndPermissions
            {
                Roles = u.Roles
                    .Select(r => r.RoleName)
                    .ToList(),

                Permissions =
                    u.Permissions.Select(p => p.Permission)
                    .Union(
                        u.Roles.SelectMany(
                            r => r.Role.RolePermissions
                                .Select(p => p.PermissionName))
                    )
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
    
    public async Task<decimal?> GetUsersDiscountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Context.UserDiscounts.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.Discount)
            .FirstOrDefaultAsync(cancellationToken);
    }
}