using Application.Common.Extensions;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Users;
using Main.Application.Handlers.Projections;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Application.Models.Cache;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;
using DbUser = Main.Entities.User.User;

namespace Main.Application.Services;

public class UserService(
    IFusionCache cache,
    IUserRepository userRepository,
    IOptions<CacheSettings> cacheSettings) : IUserService
{
    private UserCacheSettings UserSettings => cacheSettings.Value.User;

    public async Task<UserDto?> TryGetUserAsync(Guid userId, CancellationToken token = default)
    {
        var key = UserSettings.GetUserCacheKey(userId);
        return await cache.GetOrSetAsync(
            key,
            ct => TryGetUserFromDb(userId, ct),
            tags: [key],
            duration: UserSettings.Duration,
            token: token);
    }

    public async Task<decimal?> GetUserDiscountAsync(Guid userId, CancellationToken token = default)
    {
        return await cache.GetOrSetAsync(
            UserSettings.GetUserDiscountCacheKey(userId),
            ct => userRepository.GetUsersDiscountAsync(userId, ct),
            tags: [UserSettings.GetUserCacheKey(userId)],
            duration: UserSettings.Duration,
            token: token);
    }

    public async Task<UserRolesAndPermissions?> GetUserRolesAndPermissionsAsync(
        Guid userId,
        CancellationToken token = default)
    {
        return await cache.GetOrSetAsync(
            UserSettings.GetUserRolesAndPermissionsCacheKey(userId),
            ct => userRepository.GetUserRolesAndPermissionsAsync(userId, ct),
            tags: [UserSettings.GetUserCacheKey(userId), "roles"],
            duration: UserSettings.Duration,
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