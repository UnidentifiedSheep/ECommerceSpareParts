using System.Linq.Expressions;
using MonoliteUnicorn.Dtos.Amw.Articles;

namespace MonoliteUnicorn.Services.CacheService;

public interface IArticleCache
{
    /// <summary>
    /// Кеширует артикул полностью, при этом удаляет все старые данные из кеша.
    /// Note: Не удаляет места(article-used-in:) в кеше где используется данный артикул. 
    /// </summary>
    /// <param name="articleId">Id артикула</param>
    /// <returns></returns>
    Task CacheArticleFromZeroAsync(int articleId);

    /// <summary>
    /// Re Кеширует артикул полностью при условии, что он есть в кеше, при этом удаляет все старые данные из кеша.
    /// Note: Не удаляет места(article-used-in:) в кеше где используется данный артикул. 
    /// </summary>
    /// <param name="articleId">Id артикула</param>
    /// <returns></returns>
    Task CacheArticleFromZeroIfExistAsync(int articleId);

    /// <summary>
    /// Возвращает кешированные кросс-артикулы.
    /// </summary>
    /// <param name="articleId">Id артикула, для которого нужно получить кроссы</param>
    /// <param name="sortFieldSelector">Поле по которому сортировать (Работает при условии, что такая сортировка есть)</param>
    /// <param name="offset">Сколько элементов пропускать</param>
    /// <param name="limit">Сколько элементов получить</param>
    /// <param name="sort">Направление сортировки</param>
    /// <returns></returns>
    Task<(List<ArticleFullDto>, HashSet<int>)> GetFullArticleCrossesAsync(int articleId,
        Expression<Func<ArticleFullDto, object>>? sortFieldSelector = null,
        int offset = 0, int limit = 100, string? sort = "desc");
    
    /// <summary>
    /// Возвращает кешированные кросс-артикулы. При этом использует Mappster для того,
    /// чтобы привести к нужному типу.
    /// </summary>
    /// <param name="articleId">Id артикула, для которого нужно получить кроссы</param>
    /// <param name="sortFieldSelector">Поле по которому сортировать (Работает при условии, что такая сортировка есть)</param>
    /// <param name="offset">Сколько элементов пропускать</param>
    /// <param name="limit">Сколько элементов получить</param>
    /// <param name="sort">Направление сортировки</param>
    /// <returns></returns>
    Task<(List<T>, HashSet<int>)> GetAndAdaptFullArticleCrossesAsync<T>(int articleId,
        Expression<Func<ArticleFullDto, object>>? sortFieldSelector = null,
        int offset = 0, int limit = 100, string? sort = "desc");
    
    /// <summary>
    /// Обновляет данные артикулов, при условии, что артикул был в кеше.
    /// Note: Сортировки по количеству на складе, цене итд так же будут затронуты.
    /// </summary>
    /// <param name="articleIds">Ids артикулов</param>
    /// <returns></returns>
    Task ReCacheArticleModelsAsync(IEnumerable<int> articleIds);
    /// <summary>
    /// Кэширует артикулы. Не кэширует их кроссы и все остальные данные.
    /// </summary>
    /// <param name="articleIds">Ids артикулов</param>
    /// <returns></returns>
    Task CacheOnlyArticleModels(IEnumerable<int> articleIds);
}