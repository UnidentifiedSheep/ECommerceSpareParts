using Core.Extensions;
using Core.StaticFunctions;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Exceptions.Articles;
using MonoliteUnicorn.Extensions;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.Services.Catalogue;

public class Catalogue : ICatalogue
{
    private readonly DContext _context;
    public Catalogue(DContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Article>> GetArticlesByName(string searchTerm, int page, int viewCount, 
        string? sortBy, IEnumerable<int> producerIds, CancellationToken cancellationToken = default)
    {
        var normalizedSearchTerm = searchTerm.ToNormalizedArticleNumber();
        var producerIdsSet = producerIds.ToHashSet();
        var queryWithRank = _context.Articles
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
        {
            queryWithRank = sortBy?.Contains("asc") == true
                ? queryWithRank.OrderBy(x => x.Rank)
                : queryWithRank.OrderByDescending(x => x.Rank);
        }
        
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

    public async Task<IEnumerable<Article>> GetArticlesByExecNumber(string searchTerm, int page, int viewCount, 
        string? sortBy, IEnumerable<int> producerIds, CancellationToken cancellationToken = default)
    {
        var normalizerArticle = searchTerm.ToNormalizedArticleNumber();
        var producerIdsSet = producerIds.ToHashSet();
        
        var queryWithRank = _context.Articles
            .AsNoTracking()
            .Where(a => EF.Functions.Like(a.NormalizedArticleNumber, $"{normalizerArticle}") &&
                        (!producerIdsSet.Any() || producerIdsSet.Contains(a.ProducerId)))
            .Select(x => new
            {
                Article = x,
                Rank = EF.Functions.TrigramsSimilarity(x.NormalizedArticleNumber, normalizerArticle)
            });
        
        if (string.IsNullOrWhiteSpace(sortBy) || sortBy.Contains("relevance"))
        {
            queryWithRank = sortBy?.Contains("asc") == true
                ? queryWithRank.OrderBy(x => x.Rank)
                : queryWithRank.OrderByDescending(x => x.Rank);
        }
        
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

    public async Task<IEnumerable<Article>> GetArticlesByNameOrNumber(string searchTerm, int page, int viewCount, 
        string? sortBy, IEnumerable<int> producerIds, CancellationToken cancellationToken = default)
    {
        var normalizedSearchTerm = searchTerm.ToNormalizedArticleNumber();
        var producerIdsSet = producerIds.ToHashSet();
        var queryWithRank = _context.Articles
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
        {
            queryWithRank = sortBy?.Contains("asc") == true
                ? queryWithRank.OrderBy(x => x.Rank)
                : queryWithRank.OrderByDescending(x => x.Rank);
        }
        
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

    public async Task<IEnumerable<Article>> GetArticlesByStartNumber(string searchTerm, int page, int viewCount, 
        string? sortBy, IEnumerable<int> producerIds, CancellationToken cancellationToken = default)
    {
        var normalizerArticle = searchTerm.ToNormalizedArticleNumber();
        var producerIdsSet = producerIds.ToHashSet();
        
        var queryWithRank = _context.Articles
            .AsNoTracking()
            .Where(a => EF.Functions.Like(a.NormalizedArticleNumber, $"{normalizerArticle}%") &&
                        (!producerIdsSet.Any() || producerIdsSet.Contains(a.ProducerId)))
            .Select(x => new
            {
                Article = x,
                Rank = EF.Functions.TrigramsSimilarity(x.NormalizedArticleNumber, normalizerArticle)
            });
        
        if (string.IsNullOrWhiteSpace(sortBy) || sortBy.Contains("relevance"))
        {
            queryWithRank = sortBy?.Contains("asc") == true
                ? queryWithRank.OrderBy(x => x.Rank)
                : queryWithRank.OrderByDescending(x => x.Rank);
        }
        
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

    public async Task SetArticleIndicator(int articleId, string? indicator, CancellationToken token = default)
    {
        var article = await _context.Articles.FirstOrDefaultAsync(x => x.Id == articleId, token);
        if (article == null)
            throw new ArticleNotFoundException(articleId);
        indicator = string.IsNullOrWhiteSpace(indicator) ? null : indicator.Trim();
        if (!string.IsNullOrWhiteSpace(indicator) && !indicator.ColorIsValid())
            throw new IndicatorColorIsNotValidException(indicator);
        article.Indicator = indicator;
        await _context.SaveChangesAsync(token);
    }

    public async Task AddArticlesContent(int articleId, Dictionary<int, int> content, CancellationToken token = default)
    {
        if (content.ContainsKey(articleId))
            throw new ArticleContentCannotBeSameAsArticleException(articleId);

        if (content.Values.Any(x => x < 0))
            throw new ArticleContentCountCannotBeNegative();

        var ids = content.Keys.ToList();
        ids.Add(articleId);
        await _context.EnsureArticlesExist(ids, token);
        
        var values = content.Select(kv => $"({articleId}, {kv.Key}, {kv.Value})");
        var sql = $"""
                   INSERT INTO articles_content (main_article_id, inside_article_id, quantity)
                   VALUES {string.Join(", ", values)}
                   ON CONFLICT DO NOTHING;
                   """;
        await _context.Database.ExecuteSqlRawAsync(sql, token);
    }

    public async Task RemoveArticlesContent(int articleId, IEnumerable<int> insideIds, CancellationToken token = default)
    {
        var contentSet = insideIds.ToHashSet();
        if (contentSet.Count == 0)
            return;

        var idsToCheck = contentSet.ToHashSet();
        idsToCheck.Add(articleId);

        await _context.EnsureArticlesExist(idsToCheck, token);

        var idsList = contentSet.ToList();
        var idsSql = string.Join(",", idsList);

        var sql = $"""
                       DELETE FROM articles_content
                       WHERE main_article_id = {articleId}
                       AND inside_article_id IN ({idsSql});
                   """;

        await _context.Database.ExecuteSqlRawAsync(sql, token);
    }

    public async Task SetArticleContentCount(int articleId, int insideArticleId, int count,
        CancellationToken token = default)
    {
        var content = await _context.ArticlesContents
            .FirstOrDefaultAsync(x => x.MainArticleId == articleId &&
                x.InsideArticleId == insideArticleId, token);
        if (content == null)
            throw new ArticleContentNotFoundException(new {articleId, insideArticleId});
        content.Quantity = count;
        await _context.SaveChangesAsync(token);
    }
}