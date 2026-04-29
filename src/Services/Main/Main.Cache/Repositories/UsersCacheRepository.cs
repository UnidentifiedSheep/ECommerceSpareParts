using Abstractions.Interfaces.Cache;
using Application.Common.Interfaces;
using Main.Application.Dtos.Users;
using Main.Application.Interfaces.CacheRepositories;

namespace Main.Cache.Repositories;

public class UsersCacheRepository : IUsersCacheRepository
{
    private readonly ICache _redis;
    private readonly TimeSpan? _ttl;
    private readonly ICacheKeyRegistry _keyRegistry;

    public UsersCacheRepository(ICache redis, ICacheKeyRegistry keyRegistry, TimeSpan? ttl = null)
    {
        _redis = redis;
        _keyRegistry = keyRegistry;
        _ttl = ttl;
    }

    public async Task<decimal?> GetUserDiscount(Guid userId)
    {
        var key = GetUserDiscountKey(userId);
        var value = await _redis.StringGetAsync(key);
        decimal.TryParse(value, Global.Culture, out var result);
        return !string.IsNullOrWhiteSpace(value) ? result : null;
    }

    public async Task SetUserDiscount(Guid userId, decimal discount)
    {
        var key = GetUserDiscountKey(userId);
        await _redis.StringSetAsync(key, discount.ToString(Global.Culture), _ttl);
    }

    public async Task<UserDto?> GetUserById(Guid id)
    {
        var key = GetUserByIdKey(id);
        return await _redis.StringGetAsync<UserDto>(key);
    }

    public async Task SetUserById(UserDto user)
    {
        var key = GetUserByIdKey(user.Id);
        await _redis.StringSetAsync(key, user, _ttl);
    }

    public async Task<IReadOnlyList<string>> GetUserRoles(Guid userId)
    {
        var key = GetUserRolesKey(userId);
        var result = await _redis.SetMembersAsync(key);
        return result.Where(x => x != null).Select(x => x!).ToList();
    }

    public async Task SetUserRoles(Guid userId, IEnumerable<string> roles)
    {
        var key = GetUserRolesKey(userId);
        await _redis.DeleteAsync(key);
        await _redis.SetAddAsync(key, roles, _ttl);
    }
    
    public async Task<IReadOnlyList<string>> GetUserPermissions(Guid userId)
    {
        var key = GetUserPermissionsKey(userId);
        var result = await _redis.SetMembersAsync(key);
        return result.Where(x => x != null).Select(x => x!).ToList();
    }
    
    public async Task SetUserPermissions(Guid userId, IEnumerable<string> permissions)
    {
        var key = GetUserPermissionsKey(userId);
        await _redis.DeleteAsync(key);
        await _redis.SetAddAsync(key, permissions, _ttl);
    }

    private string GetUserRolesKey(Guid userId) => string.Format(CacheKeys.UserRolesCacheKey, userId);
    private static string GetUserPermissionsKey(Guid userId) => string.Format(CacheKeys.UserPermissionsCacheKey, userId);
    private static string GetUserByIdKey(Guid userId) => string.Format(CacheKeys.UserByIdCacheKey, userId);
    private static string GetUserDiscountKey(Guid userId) => string.Format(CacheKeys.UserDiscountCacheKey, userId);
}