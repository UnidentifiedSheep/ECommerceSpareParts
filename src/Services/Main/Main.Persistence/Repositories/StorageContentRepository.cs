using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class StorageContentRepository(DContext context) : IStorageContentRepository
{
    public async Task<Dictionary<int, List<decimal>>> GetHighestBuyPrices(IEnumerable<int> articleIds,
        int takePerArticle, bool calcWhereZero = false,
        CancellationToken cancellationToken = default)
    {
        var query = context.StorageContents
            .AsNoTracking()
            .Where(x => articleIds.Contains(x.ArticleId));

        if (!calcWhereZero)
            query = query.Where(x => x.Count > 0);

        var result = await query
            .GroupBy(x => x.ArticleId)
            .Select(g => new
            {
                ArticleId = g.Key,
                Prices = g
                    .OrderByDescending(x => x.BuyPriceInUsd)
                    .Take(takePerArticle)
                    .Select(x => x.BuyPriceInUsd)
                    .ToList()
            })
            .ToDictionaryAsync(x => x.ArticleId, x => x.Prices, cancellationToken);

        return result;
    }

    public async Task<IEnumerable<StorageContent>> GetStorageContentsForUpdate(IEnumerable<int> ids, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.StorageContents
            .FromSql($"SELECT * FROM storage_content where id = ANY({ids}) FOR UPDATE")
            .ConfigureTracking(track)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StorageContent>> GetStorageContents(string? storageName, int? articleId, int page,
        int viewCount, bool showZeroCount,
        bool track = true, CancellationToken cancellationToken = default)
    {
        var query = context.StorageContents
            .ConfigureTracking(true)
            .Include(x => x.Currency)
            .Where(c => string.IsNullOrWhiteSpace(storageName) || c.StorageName == storageName)
            .Where(x => articleId == null || x.ArticleId == articleId);
        if (!showZeroCount)
            query = query.Where(x => x.Count > 0);
        var result = await query
            .OrderByDescending(x => x.Count)
            .Skip(page * viewCount)
            .Take(viewCount)
            .ToListAsync(cancellationToken);
        return result;
    }

    public async Task<StorageContent?> GetStorageContentForUpdateAsync(int? id, int? articleId, string? storageName,
        bool track = true, CancellationToken cancellationToken = default)
    {
        return await context.StorageContents
            .FromSql($"""
                      SELECT * FROM storage_content
                      WHERE 
                          ({id == null} OR id = {id}) AND
                          ({articleId == null} OR article_id = {articleId}) AND
                          ({storageName == null} OR storage_name = {storageName})
                      FOR UPDATE
                      """)
            .ConfigureTracking(track)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Dictionary<(int contentId, string storageId), StorageContent>> GetStorageContentsForUpdateAsync(
        IEnumerable<(int contentId, string storageId)> ids, bool track = true,
        CancellationToken cancellationToken = default)
    {
        var parameters = new List<object>();
        var tupleConditions = new List<string>();
        var i = 0;

        foreach (var (contentId, storageId) in ids)
        {
            var idParamName = $"@p_id{i}";
            var storageParamName = $"@p_storage{i}";

            parameters.Add(new NpgsqlParameter(idParamName, contentId));
            parameters.Add(new NpgsqlParameter(storageParamName, storageId));

            tupleConditions.Add($"({idParamName}, {storageParamName})");
            i++;
        }

        var inClause = string.Join(", ", tupleConditions);

        var sql = $"""
                   SELECT * 
                   FROM storage_content
                   WHERE (id, storage_name) IN ({inClause})
                   FOR UPDATE
                   """;

        var query = context.StorageContents.FromSqlRaw(sql, parameters.ToArray());

        query = query.ConfigureTracking(track);

        var rs = await query.ToListAsync(cancellationToken);

        return rs.ToDictionary(
            x => (x.Id, x.StorageName),
            x => x
        );
    }

    public IAsyncEnumerable<StorageContent> GetStorageContentsForUpdateAsync(int? articleId, string? storageName,
        IEnumerable<int>? exceptArticleIds = null, IEnumerable<string>? exceptStorages = null, int countGreaterThen = 0,
        bool track = true)
    {
        var exceptArticles = exceptArticleIds ?? [];
        var exceptStoragesArray = exceptStorages ?? [];
        return context.StorageContents
            .FromSql($"""
                      SELECT *
                      FROM storage_content
                      WHERE ({articleId == null} OR article_id = {articleId})
                        AND (NOT (article_id = ANY({exceptArticles.ToArray()}::int[])))
                        AND ({storageName == null} OR storage_name = {storageName})
                        AND (NOT (storage_name = ANY({exceptStoragesArray.ToArray()}::text[])))
                        AND count > {countGreaterThen}
                      FOR UPDATE
                      """)
            .ConfigureTracking(track)
            .AsAsyncEnumerable();
    }

    public async Task<Dictionary<int, int>> GetStorageContentCounts(string storageName, IEnumerable<int> articleIds,
        bool takeFromOtherStorages,
        CancellationToken cancellationToken = default)
    {
        return await context.StorageContents
            .AsNoTracking()
            .Where(x => x.Count > 0 &&
                        articleIds.Contains(x.ArticleId) &&
                        (takeFromOtherStorages || x.StorageName == storageName))
            .GroupBy(x => x.ArticleId)
            .Select(g => new
            {
                ArticleId = g.Key,
                TotalCount = g.Sum(x => x.Count)
            })
            .ToDictionaryAsync(x => x.ArticleId,
                x => x.TotalCount, cancellationToken);
    }
}