using Application.Common.Extensions;
using Application.Common.Interfaces.Repositories;
using Cache;
using Cache.Extensions;
using Main.Application;
using Main.Application.Dtos.Users;
using Main.Application.Handlers.Projections;
using Main.Application.Interfaces.Cache;
using Main.Application.Interfaces.Persistence;
using Main.Entities.User;

namespace Main.Cache;

public class UserCacheRepository(
    ICache rawCache,
    IUserRepository userRepository) : IUserCacheRepository
{
    public async Task<UserDto?> TryGetUserAsync(
        Guid userId,
        CancellationToken token = default)
    {
        var key = CacheKeys.UserCache.User(userId);
        var cached = await rawCache.GetAsync<UserDto>(key);

        if (cached != null)
            return cached;

        var user = await TryGetUserFromDb(userId, token);
        if (user == null)
            return null;

        await rawCache.SetAsync(
            [(key, user)],
            CacheKeys.UserCache.Ttl);

        return user;
    }

    public async Task<decimal?> GetUserDiscountAsync(
        Guid userId,
        CancellationToken token = default)
    {
        var key = CacheKeys.UserCache.UserDiscount(userId);
        var cached = await rawCache.GetAsync<decimal?>(key);

        if (cached.HasValue)
            return cached.Value;

        var discount = await userRepository.GetUsersDiscountAsync(userId, token);

        if (discount.HasValue)
            await rawCache.SetAsync(
                [(key, discount)],
                CacheKeys.UserCache.Ttl);

        return discount;
    }

    public async Task<UserRolesAndPermissions?> GetUserRolesAndPermissionsAsync(
        Guid userId,
        CancellationToken token = default)
    {
        var key = CacheKeys.UserCache.UserRolesAndPermissions(userId);
        var cached = await rawCache.GetAsync<UserRolesAndPermissions>(key);

        if (cached != null)
            return cached;

        var rolesAndPermissions = await userRepository.GetUserRolesAndPermissionsAsync(userId, token);
        if (rolesAndPermissions == null)
            return null;

        await rawCache.SetAsync(
            [(key, rolesAndPermissions)],
            CacheKeys.UserCache.Ttl);

        await rawCache.AddToSetAsync(
            CacheKeys.UserCache.RolesAndPermissionsRelations(),
            [key],
            CacheKeys.UserCache.Ttl);

        return rolesAndPermissions;
    }

    public Task InvalidateUserAsync(Guid userId)
    {
        return rawCache.RemoveKeysAsync(
        [
            CacheKeys.UserCache.User(userId),
            CacheKeys.UserCache.UserDiscount(userId),
            CacheKeys.UserCache.UserRolesAndPermissions(userId)
        ]);
    }

    public Task InvalidateUsersAsync(IEnumerable<Guid> userIds)
    {
        return rawCache.RemoveKeysAsync(userIds.SelectMany(userId => new[]
        {
            CacheKeys.UserCache.User(userId),
            CacheKeys.UserCache.UserDiscount(userId),
            CacheKeys.UserCache.UserRolesAndPermissions(userId)
        }));
    }

    public Task InvalidateUserDiscountAsync(Guid userId)
    {
        return rawCache.RemoveKeyAsync(CacheKeys.UserCache.UserDiscount(userId));
    }

    public Task InvalidateUserRolesAndPermissionsAsync(Guid userId)
    {
        return rawCache.RemoveKeyAsync(CacheKeys.UserCache.UserRolesAndPermissions(userId));
    }

    public Task InvalidateRolesAsync()
    {
        return rawCache.InvalidateByRelationsAsync(CacheKeys.UserCache.RolesAndPermissionsRelations());
    }

    private async Task<UserDto?> TryGetUserFromDb(Guid userId, CancellationToken token)
    {
        var criteria = Criteria<User>.New()
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
