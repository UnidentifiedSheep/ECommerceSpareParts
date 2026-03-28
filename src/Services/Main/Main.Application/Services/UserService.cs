using Abstractions.Interfaces.RelatedData;
using Abstractions.Models.Repository;
using Main.Abstractions.Constants;
using Main.Abstractions.Dtos.Users;
using Main.Abstractions.Interfaces.CacheRepositories;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Abstractions.Interfaces.Services;
using Main.Entities;

namespace Main.Application.Services;

public class UserService(
    IRelatedDataFactory relatedDataFactory, 
    IUsersCacheRepository cacheRepository,
    IUserRepository userRepository,
    IUserPermissionRepository userPermissionRepository,
    IUserRoleRepository userRoleRepository) : IUserService
{
    private static readonly QueryOptions<User> UserQueryOptions = new QueryOptions<User>()
        .WithInclude(x => x.UserInfo)
        .WithTracking(false);

    private static readonly PageableQueryOptions<UserRole> UserRoleQueryOptions = new PageableQueryOptions<UserRole>()
        .WithTracking(false)
        .WithInclude(x => x.Role)
        .WithInclude(x => x.Role.PermissionNames);
    public async Task<User?> TryGetUserAsync(Guid userId, CancellationToken token = default)
    {
        var cachedUser = await cacheRepository.GetUserById(userId);
        if (cachedUser != null) return cachedUser;

        var dbUser = await userRepository.GetUserByIdAsync(userId, UserQueryOptions, token);
        
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

    public async Task<UserRolesAndPermissions> TryGetUserRolesAndPermissionsAsync(
        Guid userId, 
        CancellationToken token = default)
    {
        var permissions = (await cacheRepository.GetUserPermissions(userId)).ToHashSet();
        var roles = (await cacheRepository.GetUserRoles(userId)).ToHashSet();
        
        if (roles.Count == 0)
        {
            var dbRoles = await userRoleRepository
                .GetUserRolesAsync(userId, UserRoleQueryOptions, token);
            var rolesPermissions = dbRoles
                .SelectMany(x => x.Role.PermissionNames)
                .Select(x => x.Name)
                .ToList();
            var userPermissions = await userPermissionRepository
                .GetUserPermissionNamesAsync(userId, token);
            
            roles.Clear();
            roles.UnionWith(dbRoles.Select(x => x.Role.Name));
            
            permissions.Clear();
            permissions.UnionWith(userPermissions);
            permissions.UnionWith(rolesPermissions);
        }

        return new UserRolesAndPermissions
        {
            Permissions = permissions.ToList(),
            Roles = roles.ToList()
        };
    }

    private async Task SaveUserInCache(User user)
    {
        await AddRelatedDataKeys(user.Id);
        await cacheRepository.SetUserById(user);
    }

    private async Task AddRelatedDataKeys(Guid userId)
    {
        var relatedRepository = relatedDataFactory.GetRepository<User>();
        var relatedDataKeys = new List<string>
        {
            string.Format(CacheKeys.UserRolesCacheKey, userId),
            string.Format(CacheKeys.UserPermissionsCacheKey, userId)
        };
        await relatedRepository.AddRelatedDataAsync(userId.ToString(), relatedDataKeys);
    }
}