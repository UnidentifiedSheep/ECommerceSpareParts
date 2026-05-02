namespace Main.Application.Models.Cache;

public record UserCacheSettings : CacheSetting
{
    public string GetUserCacheKey(Guid userId) => $"user:{userId}";
    public string GetUserDiscountCacheKey(Guid userId) => $"user:{userId}:discount";
    public string GetUserRolesAndPermissionsCacheKey(Guid userId) => $"user:{userId}:roles:permissions";
}