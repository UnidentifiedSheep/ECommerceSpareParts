using System.Linq.Expressions;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Articles;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.RedisFunctions;
using Polly;
using Polly.Retry;
using Serilog;
using StackExchange.Redis;

namespace MonoliteUnicorn.Services.CacheService;

public class ArticleCache : IArticleCache
{
    private static readonly AsyncRetryPolicy RetryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt)),
            onRetry: (exception, _, retryCount, _) =>
            {
                Log.Error(exception, $"[Retry {retryCount}] Ошибка обновления данных redis");
            });

    private readonly DContext _context;
    private readonly IRedisArticleRepository _redisArticles;

    public ArticleCache(DContext context, IRedisArticleRepository redisArticles)
    {
        _context = context;
        _redisArticles = redisArticles;
    }

    public async Task ReCacheArticleModelsAsync(IEnumerable<int> articleIds)
    {
        var ids = articleIds.Distinct().ToList();
        if (ids.Count == 0) return;
        var articles = await _context.Articles.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Include(x => x.ArticleImages)
            .Include(x => x.Producer)
            .ToListAsync();
        if (articles.Count == 0) return;
        await RetryPolicy.ExecuteAsync(() => _redisArticles.UpdateArticlesWhichContains(articles));
    }

    public async Task CacheArticleFromZeroIfExistAsync(int articleId)
    {
        ArticleFullDto? article = null;
        await RetryPolicy.ExecuteAsync(async () =>
        {
            var (_, art) = (await _redisArticles.GetArticles([articleId])).FirstOrDefault();
            article = art;
        });
        if (article == null) return;
        await CacheArticleFromZeroAsync(articleId);
    }

    public async Task CacheArticleFromZeroAsync(int articleId)
    {
        var fullCrosses = await _context.Articles
            .FromSql($"""
                      SELECT Distinct on (a.id) a.* 
                      FROM articles a 
                      JOIN article_crosses c ON a.id = c.article_id OR a.id = c.article_cross_id 
                                             WHERE c.article_id = {articleId} OR 
                                                 c.article_cross_id = {articleId} 
                      """)
            .AsNoTracking()
            .Include(x => x.ArticleImages)
            .Include(x => x.Producer)
            .ToListAsync();
        var crossIds = fullCrosses
            .Select(x => x.Id)
            .ToHashSet();
        crossIds.Remove(articleId);
        var asSortedByCount = fullCrosses
            .Select(x => new SortedSetEntry(x.Id, x.TotalCount))
            .ToArray();

        await RetryPolicy.ExecuteAsync(async () =>
        {
            await _redisArticles.ClearUsableArticleData(articleId);
            var addCrossesTask = _redisArticles.AddArticleCrosses(articleId, crossIds);
            var addArticlesTask = _redisArticles.SetArticles(fullCrosses);
            var addArticleCrossesWithSortTask =
                _redisArticles.AddArticleCrossesWithSort(articleId, asSortedByCount, x => x.CurrentStock);

            await Task.WhenAll(addCrossesTask, addArticlesTask, addArticleCrossesWithSortTask);
        });
    }

    public async Task CacheOnlyArticleModels(IEnumerable<int> articleIds)
    {
        var ids = articleIds.Distinct().ToList();
        var articles = await _context.Articles.Where(x => ids.Contains(x.Id))
            .AsNoTracking()
            .Include(x => x.Producer)
            .Include(x => x.ArticleImages)
            .ToListAsync();
        await RetryPolicy.ExecuteAsync(async () =>
            await _redisArticles.SetArticles(articles));
    }

    public async Task<(List<ArticleFullDto>, HashSet<int>)> GetFullArticleCrossesAsync(int articleId, Expression<Func<ArticleFullDto, object>>? sortFieldSelector = null, 
        int offset = 0, int limit = 100, string? sort = "desc")
    {
        HashSet<int> crosses = sortFieldSelector == null
            ? await _redisArticles.GetArticleCrosses(articleId, offset, limit)
            : await _redisArticles.GetArticleCrossesWithSort(articleId, sortFieldSelector, offset, limit, sort ?? "desc");
        var articles = await _redisArticles.GetArticles(crosses);
        var notFound = new HashSet<int>();
        var found = new List<ArticleFullDto>();
        foreach (var (key, value) in articles)
        {
            if (value != null)
                found.Add(value);
            else
                notFound.Add(key);
        }
        return (found, notFound);
    }

    public async Task<(List<T>, HashSet<int>)> GetAndAdaptFullArticleCrossesAsync<T>(int articleId, Expression<Func<ArticleFullDto, object>>? sortFieldSelector = null, int offset = 0,
        int limit = 100, string? sort = "desc")
    {
        var (found, notFound) = await GetFullArticleCrossesAsync(articleId,sortFieldSelector, offset, limit, sort);
        return (found.Adapt<List<T>>(), notFound);
    }
}