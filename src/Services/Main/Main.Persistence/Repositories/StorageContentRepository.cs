using Main.Abstractions.Interfaces.DbRepositories;
using Main.Abstractions.Models;
using Main.Entities;
using Main.Entities.Storage;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class StorageContentRepository(DContext context) : IStorageContentRepository
{

    public async Task<IEnumerable<StorageContent>> GetStorageContentsForUpdate(
        IEnumerable<int> ids,
        bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.StorageContents
            .FromSql($"SELECT * FROM storage_content where id = ANY({ids}) FOR UPDATE")
            .ConfigureTracking(track)
            .ToListAsync(cancellationToken);
    }

    public async Task<StorageContent?> GetStorageContentForUpdateAsync(
        int? id,
        int? articleId,
        string? storageName,
        bool track = true,
        CancellationToken cancellationToken = default)
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
        IEnumerable<(int contentId, string storageId)> ids,
        bool track = true,
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

    

    
}