using Application.Common.Extensions;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Users;
using Main.Application.Handlers.Projections;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using ZiggyCreatures.Caching.Fusion;
using DbUser = Main.Entities.User.User;

namespace Main.Application.Services;

public class UserService(
    IFusionCache cache,
    IUserRepository userRepository) : IUserService
{
    public async Task<UserDto?> TryGetUserAsync(Guid userId, CancellationToken token = default)
    {
        var key = CacheKeys.UserCache.GetUserCacheKey(userId);
        return await cache.GetOrSetAsync(
            key,
            ct => TryGetUserFromDb(userId, ct),
            tags: [key],
            duration: CacheKeys.UserCache.Ttl,
            token: token);
    }

    public async Task<decimal?> GetUserDiscountAsync(Guid userId, CancellationToken token = default)
    {
        return await cache.GetOrSetAsync(
            CacheKeys.UserCache.GetUserDiscountCacheKey(userId),
            ct => userRepository.GetUsersDiscountAsync(userId, ct),
            tags: [CacheKeys.UserCache.GetUserCacheKey(userId)],
            duration: CacheKeys.UserCache.Ttl,
            token: token);
    }

    public async Task<UserRolesAndPermissions?> GetUserRolesAndPermissionsAsync(
        Guid userId,
        CancellationToken token = default)
    {
        return await cache.GetOrSetAsync(
            CacheKeys.UserCache.GetUserRolesAndPermissionsCacheKey(userId),
            ct => userRepository.GetUserRolesAndPermissionsAsync(userId, ct),
            tags: [CacheKeys.UserCache.GetUserCacheKey(userId), "roles"],
            duration: CacheKeys.UserCache.Ttl,
            token: token);
    }


    private async Task<UserDto?> TryGetUserFromDb(Guid userId, CancellationToken token)
    {
        var criteria = Criteria<DbUser>.New()
            .Where(x => x.Id == userId)
            .Include(x => x.UserInfo)
            .Track(false)
            .Build();

        var user = await userRepository.FirstOrDefaultAsync(criteria, token);
        return user == null
            ? null
            : UserProjections.UserProjection.AsFunc()(user);
    }
}