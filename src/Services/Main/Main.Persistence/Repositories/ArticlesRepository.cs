using System.Text;
using Core.Extensions;
using Main.Core.Entities;
using Main.Core.Extensions;
using Main.Core.Interfaces.DbRepositories;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class ArticlesRepository(DContext context) : IArticlesRepository
{
    public async Task<List<Article>> GetArticlesByName(string searchTerm, int page, int viewCount,
        string? sortBy, IEnumerable<int> producerIds, CancellationToken cancellationToken = default)
    {
        var producerIdsSet = producerIds.ToHashSet();
        var query = context.Articles
            .AsNoTracking()
            .Where(x =>
                EF.Property<NpgsqlTsVector>(x, "articlename_tsv")
                    .Matches(EF.Functions.PlainToTsQuery("russian", searchTerm)));

        if (producerIdsSet.Any())
            query = query.Where(x => producerIdsSet.Contains(x.ProducerId));
        

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
        return await context.Articles
            .FromSql($"""
                      SELECT DISTINCT a.id
                      FROM articles a
                      JOIN article_crosses c
                        ON a.id = c.article_id
                        OR a.id = c.article_cross_id
                      WHERE c.article_id = {articleId}
                         OR c.article_cross_id = {articleId}
                      """)
            .AsNoTracking()
            .Select(x => x.Id)
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
}