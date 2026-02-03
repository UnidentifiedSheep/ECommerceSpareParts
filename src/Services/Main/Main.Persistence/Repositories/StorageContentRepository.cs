using System.Linq.Expressions;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Abstractions.Models;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class StorageContentRepository(DContext context) : IStorageContentRepository
{
    public async Task<Dictionary<int, List<StorageContentLogisticsProjection>>> GetStorageContentsForPricing(IEnumerable<int> articleIds, 
        bool onlyPositiveQty = true, CancellationToken ct = default, params Expression<Func<StorageContent, object?>>[] includes)
    {
        var list = await context.Database
            .SqlQuery<StorageContentLogisticsProjection>($"""
                                                          SELECT 
                                                              sc.Id AS StorageContentId,
                                                              sc.article_id AS ArticleId,
                                                              sc.currency_id AS CurrencyId,
                                                              sc.buy_price AS Price,
                                                              pl.currency_id AS LogisticsCurrencyId,
                                                              pcl.price AS LogisticsPrice,
                                                              pc.id AS PurchaseContentId,
                                                              pc.count AS PurchaseContentCount,
                                                              p.id AS PurchaseId
                                                          FROM storage_content sc
                                                          LEFT JOIN purchase_content pc ON sc.id = pc.storage_content_id
                                                          LEFT JOIN purchase p ON pc.purchase_id = p.Id
                                                          LEFT JOIN purchase_logistics pl ON p.id = pl.purchase_id
                                                          LEFT JOIN purchase_content_logistics pcl ON pc.id = pcl.purchase_content_id
                                                          WHERE sc.article_id = ANY({articleIds})
                                                            AND ({onlyPositiveQty} OR sc.count > 0)
                                                          """)
            .ToListAsync(ct);

        var result = list
            .GroupBy(x => x.ArticleId)
            .ToDictionary(g => g.Key, g => g.ToList());
        
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