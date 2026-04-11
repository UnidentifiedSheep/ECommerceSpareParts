using Abstractions.Models.Repository;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Entities.Auth;
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
}