namespace Main.Abstractions.Constants;

public static class CacheKeys
{
    /// <summary>
    /// 1 - ArticleId, 2 - Page, 3 - ViewCount, 4 - SortBy
    /// </summary>
    public const string ArticleCrossesCacheKey = "article-crosses:{0}-{1}-{2}-{3}";

    /// <summary>
    /// 0 - SearchTerm, 1 - Page, 2 - ViewCount, 3 - SortBy
    /// </summary>
    public const string ArticlesCacheKey = "articles:{0}-{1}-{2}-{3}";

    /// <summary>
    /// 0 - ArticleId
    /// </summary>
    public const string ArticleWeightCacheKey = "article-weight:{0}";

    /// <summary>
    /// 0 - ArticleId
    /// </summary>
    public const string ArticleSizeCacheKey = "article-size:{0}";

    /// <summary>
    /// 0 - Page, 1 - Limit
    /// </summary>
    public const string CurrenciesCacheKey = "currencies:{0}-{1}";

    public const string CurrencyRatesCacheKey = "currency-rates";

    /// <summary>
    /// 0 - from storage name, 1 - to storage name
    /// </summary>
    public const string ActiveStorageRouteCacheKey = "active-storage-route:{0}-{1}";
    
    public const string UserByIdCacheKey = "user-by-id:{0}";
    public const string UserDiscountCacheKey = "user-discount:{0}";
    public const string UserPermissionsCacheKey = "user-permissions:{0}";
    public const string UserRolesCacheKey = "user-roles:{0}";
}