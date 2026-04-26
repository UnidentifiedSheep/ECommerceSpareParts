using Abstractions.Interfaces.RelatedData;
using Application.Common.Interfaces.Repositories;
using Main.Abstractions.Constants;
using Main.Application.Dtos.Users;
using Main.Application.Handlers.Projections;
using Main.Application.Interfaces.CacheRepositories;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using DbUser = Main.Entities.User.User;

namespace Main.Application.Services;

public class UserService(
    IRelatedDataFactory relatedDataFactory, 
    IUsersCacheRepository cacheRepository,
    IUserRepository userRepository) : IUserService
{
    public async Task<UserDto?> TryGetUserAsync(Guid userId, CancellationToken token = default)
    {
        var cachedUser = await cacheRepository.GetUserById(userId);
        if (cachedUser != null) return cachedUser;

        var dbUser = await TryGetUserFromDb(userId, token);
        if (dbUser != null) await SaveUserInCache(dbUser);
        
        return dbUser;
    }

    public async Task<decimal?> GetUserDiscountAsync(Guid userId, CancellationToken token = default)
    {
        var cacheValue = await cacheRepository.GetUserDiscount(userId);
        if (cacheValue != null) return cacheValue;
        var dbDiscount = await userRepository.GetUsersDiscountAsync(userId, token);
        return dbDiscount;
    }

    public async Task<UserRolesAndPermissions> GetUserRolesAndPermissionsAsync(
        Guid userId, 
        CancellationToken token = default)
    {
        var rp = new UserRolesAndPermissions
        {
            Permissions = await cacheRepository.GetUserPermissions(userId),
            Roles = await cacheRepository.GetUserRoles(userId)
        };

        if (rp.Roles.Count != 0 && rp.Permissions.Count != 0)
            return rp;
        
        rp = await userRepository.GetUserRolesAndPermissionsAsync(userId, token);
        if (rp == null) return new UserRolesAndPermissions
        {
            Permissions = [],
            Roles = []
        };
            
        await cacheRepository.SetUserRoles(userId, rp.Roles);
        await cacheRepository.SetUserPermissions(userId, rp.Permissions);

        return rp;
    }

    private async Task<UserDto?> TryGetUserFromDb(Guid userId, CancellationToken token)
    {
        var criteria = Criteria<DbUser>.New()
            .Where(x => x.Id == userId)
            .Include(x => x.UserInfo)
            .Track(false)
            .Build();
        
        DbUser? user = await userRepository.FirstOrDefaultAsync(criteria, token);
        if (user == null) return null;
        return UserProjections.UserProjectionFunc(user);
    }

    private async Task SaveUserInCache(UserDto user)
    {
        await AddRelatedDataKeys(user.Id);
        await cacheRepository.SetUserById(user);
    }

    private async Task AddRelatedDataKeys(Guid userId)
    {
        var relatedRepository = relatedDataFactory.GetRepository<DbUser>();
        var relatedDataKeys = new List<string>
        {
            string.Format(CacheKeys.UserRolesCacheKey, userId),
            string.Format(CacheKeys.UserPermissionsCacheKey, userId),
            string.Format(CacheKeys.UserByIdCacheKey, userId),
        };
        await relatedRepository.AddRelatedDataAsync(userId.ToString(), relatedDataKeys);
    }
}