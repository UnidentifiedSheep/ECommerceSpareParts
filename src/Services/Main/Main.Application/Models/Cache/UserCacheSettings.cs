namespace Main.Application.Models.Cache;

public record UserCacheSettings : CacheSetting
{
    public string GetUserCacheKey(Guid userId)
    {
        return $"user:{userId}";
    }

    public string GetUserDiscountCacheKey(Guid userId)
    {
        return $"user:{userId}:discount";
    }

    public string GetUserRolesAndPermissionsCacheKey(Guid userId)
    {
        return $"user:{userId}:roles:permissions";
    }
}