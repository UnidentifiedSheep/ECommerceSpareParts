using System.Text;
using Core.Extensions;
using Main.Core.Entities;
using Main.Core.Interfaces.DbRepositories;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class ArticlesRepository(DContext context) : IArticlesRepository
{
    public async Task<List<Article>> GetArticlesByName(string searchTerm, int page, int viewCount,
        string? sortBy, IEnumerable<int> producerIds, CancellationToken cancellationToken = default)
    {
        var normalizedSearchTerm = searchTerm.ToNormalizedArticleNumber();
        var producerIdsSet = producerIds.ToHashSet();
        var queryWithRank = context.Articles
            .AsNoTracking()
            .Where(x =>
                EF.Functions.ToTsVector("russian", x.ArticleName)
                    .Matches(EF.Functions.PlainToTsQuery("russian", searchTerm)))
            .Select(x => new
            {
                Article = x,
                Rank = Math.Max(
                    EF.Functions.ToTsVector("russian", x.ArticleName)
                        .Rank(EF.Functions.PlainToTsQuery("russian", searchTerm)),
                    EF.Functions.ToTsVector("russian", x.NormalizedArticleNumber)
                        .Rank(EF.Functions.PlainToTsQuery("russian", normalizedSearchTerm))
                )
            });

        if (producerIdsSet.Any())
            queryWithRank = queryWithRank.Where(x => producerIdsSet.Contains(x.Article.ProducerId));


        if (string.IsNullOrWhiteSpace(sortBy) || sortBy.Contains("relevance"))
            queryWithRank = sortBy?.Contains("asc") == true
                ? queryWithRank.OrderBy(x => x.Rank)
                : queryWithRank.OrderByDescending(x => x.Rank);

        var query = queryWithRank
            .Select(x => x.Article);

        if (!string.IsNullOrWhiteSpace(sortBy) && !sortBy.Contains("relevance"))
            query = query.SortBy(sortBy);

        query = query
            .Skip(viewCount * page)
            .Take(viewCount);

        var articles = await query
            .AsSplitQuery()
            .Include(x => x.Producer)
            .ToListAsync(cancellationToken);
        return articles;
    }

    public async Task<List<Article>> GetArticlesByExecNumber(string searchTerm, int page, int viewCount,
        string? sortBy, IEnumerable<int> producerIds, CancellationToken cancellationToken = default)
    {
        var normalizerArticle = searchTerm.ToNormalizedArticleNumber();
        var producerIdsSet = producerIds.ToHashSet();

        var queryWithRank = context.Articles
            .AsNoTracking()
            .Where(a => EF.Functions.Like(a.NormalizedArticleNumber, $"{normalizerArticle}") &&
                        (!producerIdsSet.Any() || producerIdsSet.Contains(a.ProducerId)))
            .Select(x => new
            {
                Article = x,
                Rank = EF.Functions.TrigramsSimilarity(x.NormalizedArticleNumber, normalizerArticle)
            });

        if (string.IsNullOrWhiteSpace(sortBy) || sortBy.Contains("relevance"))
            queryWithRank = sortBy?.Contains("asc") == true
                ? queryWithRank.OrderBy(x => x.Rank)
                : queryWithRank.OrderByDescending(x => x.Rank);

        var query = queryWithRank
            .Select(x => x.Article);

        if (!string.IsNullOrWhiteSpace(sortBy) && !sortBy.Contains("relevance"))
            query = query.SortBy(sortBy);

        query = query
            .Skip(viewCount * page)
            .Take(viewCount);

        return await query
            .AsSplitQuery()
            .Include(x => x.Producer)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Article>> GetArticlesByNameOrNumber(string searchTerm, int page, int viewCount,
        string? sortBy, IEnumerable<int> producerIds, CancellationToken cancellationToken = default)
    {
        var normalizedSearchTerm = searchTerm.ToNormalizedArticleNumber();
        var producerIdsSet = producerIds.ToHashSet();
        var queryWithRank = context.Articles
            .AsNoTracking()
            .Where(x =>
                EF.Functions.ToTsVector("russian", x.ArticleName)
                    .Matches(EF.Functions.PlainToTsQuery("russian", searchTerm)) ||
                EF.Functions.ILike(x.NormalizedArticleNumber, $"%{normalizedSearchTerm}%"))
            .Select(x => new
            {
                Article = x,
                Rank = Math.Max(
                    EF.Functions.ToTsVector("russian", x.ArticleName)
                        .Rank(EF.Functions.PlainToTsQuery("russian", searchTerm)),
                    EF.Functions.ToTsVector("russian", x.NormalizedArticleNumber)
                        .Rank(EF.Functions.PlainToTsQuery("russian", normalizedSearchTerm))
                )
            });

        if (producerIdsSet.Any())
            queryWithRank = queryWithRank.Where(x => producerIdsSet.Contains(x.Article.ProducerId));


        if (string.IsNullOrWhiteSpace(sortBy) || sortBy.Contains("relevance"))
            queryWithRank = sortBy?.Contains("asc") == true
                ? queryWithRank.OrderBy(x => x.Rank)
                : queryWithRank.OrderByDescending(x => x.Rank);

        var query = queryWithRank
            .Select(x => x.Article);

        if (!string.IsNullOrWhiteSpace(sortBy) && !sortBy.Contains("relevance"))
            query = query.SortBy(sortBy);

        query = query
            .Skip(viewCount * page)
            .Take(viewCount);

        return await query
            .AsSplitQuery()
            .Include(x => x.Producer)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Article>> GetArticleCrosses(int articleId, int page, int viewCount, string? sortBy,
        bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.Articles
            .FromSql($"""
                      SELECT Distinct on (a.id) a.* 
                      FROM articles a 
                      JOIN article_crosses c ON a.id = c.article_id OR a.id = c.article_cross_id 
                                             WHERE c.article_id = {articleId} OR 
                                                 c.article_cross_id = {articleId} 
                      """)
            .SortBy(sortBy)
            .Skip(page * viewCount)
            .Include(x => x.ArticleImages)
            .Take(viewCount)
            .AsNoTracking()
            .Include(x => x.Producer)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Article>> GetArticlesByStartNumber(string searchTerm, int page, int viewCount,
        string? sortBy, IEnumerable<int> producerIds, CancellationToken cancellationToken = default)
    {
        var normalizerArticle = searchTerm.ToNormalizedArticleNumber();
        var producerIdsSet = producerIds.ToHashSet();

        var queryWithRank = context.Articles
            .AsNoTracking()
            .Where(a => EF.Functions.Like(a.NormalizedArticleNumber, $"{normalizerArticle}%") &&
                        (!producerIdsSet.Any() || producerIdsSet.Contains(a.ProducerId)))
            .Select(x => new
            {
                Article = x,
                Rank = EF.Functions.TrigramsSimilarity(x.NormalizedArticleNumber, normalizerArticle)
            });

        if (string.IsNullOrWhiteSpace(sortBy) || sortBy.Contains("relevance"))
            queryWithRank = sortBy?.Contains("asc") == true
                ? queryWithRank.OrderBy(x => x.Rank)
                : queryWithRank.OrderByDescending(x => x.Rank);

        var query = queryWithRank
            .Select(x => x.Article);

        if (!string.IsNullOrWhiteSpace(sortBy) && !sortBy.Contains("relevance"))
            query = query.SortBy(sortBy);

        query = query
            .Skip(viewCount * page)
            .Take(viewCount);

        return await query
            .AsSplitQuery()
            .Include(x => x.Producer)
            .ToListAsync(cancellationToken);
    }

    public async Task<Article?> GetArticleById(int id, bool track = true, CancellationToken cancellationToken = default)
    {
        return await context.Articles.ConfigureTracking(track)
            .Include(x => x.Producer)
            .Include(x => x.ArticleImages)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<Article>> GetArticlesByIds(IEnumerable<int> ids, bool track = true,
        CancellationToken token = default)
    {
        return await context.Articles.ConfigureTracking(track)
            .Include(x => x.Producer)
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(token);
    }

    public async Task<List<Article>> GetArticlesCrosses(int articleId, int page = -1, int viewCount = -1,
        CancellationToken token = default)
    {
        var query = context.Articles
            .FromSql($"""
                      SELECT Distinct on (a.id) a.* 
                      FROM articles a 
                      JOIN article_crosses c ON a.id = c.article_id OR a.id = c.article_cross_id 
                                             WHERE c.article_id = {articleId} OR 
                                                 c.article_cross_id = {articleId} 
                      """)
            .Include(x => x.ArticleImages)
            .Include(x => x.Producer)
            .AsNoTracking();

        if (page >= 0 && viewCount > 0)
            query = query.Skip(page * viewCount).Take(viewCount);
        return await query.ToListAsync(token);
    }

    public async Task<IEnumerable<Article>> GetArticlesForUpdate(IEnumerable<int> articleIds, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.Articles
            .FromSql($"SELECT * from articles where id = ANY ({articleIds}) for update")
            .ConfigureTracking(track)
            .ToListAsync(cancellationToken);
    }

    public async Task AddArticleLinkage(IEnumerable<(int id, int crossId)> crossIds,
        CancellationToken cancellationToken = default)
    {
        var hs = crossIds.ToHashSet();
        var queryBuilder = new StringBuilder("INSERT INTO article_crosses (article_id, article_cross_id) VALUES ");
        foreach (var (l, r) in hs)
            queryBuilder.Append($" ({l}, {r}),");
        queryBuilder.Length--;
        queryBuilder.Append(" ON CONFLICT DO NOTHING");
        await context.Database.ExecuteSqlRawAsync(queryBuilder.ToString(), cancellationToken);
    }

    public async Task<IEnumerable<int>> GetArticleCrossIds(int articleId, CancellationToken cancellationToken = default)
    {
        return await context.ArticleCrosses
            .AsNoTracking()
            .Where(x => x.ArticleId == articleId || x.ArticleCrossId == articleId)
            .Select(x => x.ArticleId)
            .Union(
                context.ArticleCrosses
                    .Where(x => x.ArticleId == articleId || x.ArticleCrossId == articleId)
                    .Select(x => x.ArticleCrossId)
            )
            .Distinct()
            .ToHashSetAsync(cancellationToken);
    }

    public async Task<int> UpdateArticlesCount(Dictionary<int, int> toIncrement,
        CancellationToken cancellationToken = default)
    {
        var valuesSql = string.Join(", ", toIncrement.Select(i => $"({i.Key}, {i.Value})"));

        var query = $"""
                     UPDATE articles a
                     SET total_count = a.total_count + data.increment
                     FROM (VALUES {valuesSql}) AS data(article_id, increment)
                     WHERE a.id = data.article_id
                     """;
        return await context.Database.ExecuteSqlRawAsync(query, cancellationToken);
    }

    public async Task<IEnumerable<int>> ArticlesExistsAsync(IEnumerable<int> ids,
        CancellationToken cancellationToken = default)
    {
        var idsSet = ids.ToHashSet();
        var exists = await context.Articles.AsNoTracking()
            .Where(x => idsSet.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
        return idsSet.Except(exists);
    }
}