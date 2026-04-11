using Abstractions.Interfaces.RelatedData;
using Abstractions.Models.Repository;
using Main.Abstractions.Constants;
using Main.Abstractions.Dtos.Users;
using Main.Abstractions.Interfaces.CacheRepositories;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Abstractions.Interfaces.Services;
using Main.Entities;
using Main.Entities.Auth;
using Mapster;
using DbUser = Main.Entities.User.User;

namespace Main.Application.Services;

public class UserService(
    IRelatedDataFactory relatedDataFactory, 
    IUsersCacheRepository cacheRepository,
    IUserRepository userRepository,
    IUserPermissionRepository userPermissionRepository,
    IUserRoleRepository userRoleRepository) : IUserService
{
    public async Task<FullUserDto?> TryGetUserAsync(Guid userId, CancellationToken token = default)
    {
        var cachedUser = await cacheRepository.GetUserById(userId);
        if (cachedUser != null) return cachedUser;

        var queryOptions = new QueryOptions<DbUser, Guid>()
            {
                Data = userId
            }
            .WithInclude(x => x.UserInfo)
            .WithTracking(false);
        var dbUser = await userRepository.GetUserByIdAsync(queryOptions, token);
        var adapted = dbUser.Adapt<FullUserDto>();
        if (adapted != null) await SaveUserInCache(adapted);
        
        return adapted;
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
        var permissions = (await cacheRepository.GetUserPermissions(userId)).ToHashSet();
        var roles = (await cacheRepository.GetUserRoles(userId)).ToHashSet();
        
        if (roles.Count == 0 || permissions.Count == 0)
        {
            var queryOptions = new QueryOptions<UserRole, Guid>()
                {
                    Data = userId
                }
                .WithTracking(false)
                .WithInclude(x => x.Role)
                .WithInclude(x => x.Role.PermissionNames);
            
            var dbRoles = await userRoleRepository
                .GetUserRolesAsync(queryOptions, token);
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

        await cacheRepository.SetUserRoles(userId, roles);
        await cacheRepository.SetUserPermissions(userId, permissions);

        return new UserRolesAndPermissions
        {
            Permissions = permissions.ToList(),
            Roles = roles.ToList()
        };
    }

    private async Task SaveUserInCache(FullUserDto user)
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