namespace Main.Abstractions.Constants;

public static class CacheKey
{
    /// <summary>
    ///     0 - SearchTerm, 1 - Page, 2 - ViewCount, 3 - SortBy
    /// </summary>
    public const string ProductsCacheKey = "products:{0}-{1}-{2}-{3}";

    /// <summary>
    ///     0 - from storage name, 1 - to storage name
    /// </summary>
    public const string ActiveStorageRouteCacheKey = "active-storage-route:{0}-{1}";

    public const string UserByIdCacheKey = "user-by-id:{0}";
    public const string UserDiscountCacheKey = "user-discount:{0}";
    public const string UserPermissionsCacheKey = "user-permissions:{0}";
    public const string UserRolesCacheKey = "user-roles:{0}";
}