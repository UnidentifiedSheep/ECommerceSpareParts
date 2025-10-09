namespace Core.StaticFunctions;

public static class CacheKeys
{
    /// <summary>
    ///     0 - ArticleId, 1 - Page, 2 - ViewCount, 3 - SortBy
    /// </summary>
    public const string ArticleCrossesCacheKey = "article-crosses:{0}-{1}-{2}-{3}";
    
    /// <summary>
    ///     0 - SearchTerm, 1 - Page, 2 - ViewCount, 3 - SortBy
    /// </summary>
    public const string ArticlesCacheKey = "articles:{0}-{1}-{2}-{3}";
    
    /// <summary>
    ///     0 - Page, 1 - Limit
    /// </summary>
    public const string CurrenciesCacheKey = "currencies:{0}-{1}";
}