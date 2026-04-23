using Main.Application.Interfaces.Persistence;
using Main.Entities.Storage;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Extensions;

namespace Main.Persistence.Repositories.Storage;

public class StorageContentRepository(DContext context) 
    : RepositoryBase<DContext, StorageContent, int>(context), IStorageContentRepository
{
    public IAsyncEnumerable<StorageContent> GetStorageContentsForUpdateAsync(
        int? productId,
        string? storageName,
        IEnumerable<int>? exceptProductIds = null,
        IEnumerable<string>? exceptStorages = null,
        int countGreaterThen = 0)
    {
        var exceptArticles = exceptProductIds?.ToList();
        var exceptStorageNames = exceptStorages?.ToList();
        var query = Context.StorageContents
            .Where(x => x.Count > countGreaterThen);

        if (productId != null)
            query = query.Where(x => x.ProductId == productId);

        if (exceptArticles != null && exceptArticles.Count != 0)
            query = query.Where(x => !exceptArticles.Contains(x.ProductId));

        if (storageName != null)
            query = query.Where(x => x.StorageName == storageName);

        if (exceptStorageNames != null && exceptStorageNames.Count != 0)
            query = query.Where(x => !exceptStorageNames.Contains(x.StorageName));

        return query
            .OrderBy(x => x.PurchaseDatetime)
            .ForUpdate()
            .AsAsyncEnumerable();
    }
    
    public async Task<Dictionary<int, int>> GetStorageContentCounts(
        string storageName,
        IEnumerable<int> productIds,
        bool takeFromOtherStorages,
        CancellationToken cancellationToken = default)
    {
        return await Context.StorageContents
            .AsNoTracking()
            .Where(x => x.Count > 0 &&
                        productIds.Contains(x.ProductId) &&
                        (takeFromOtherStorages || x.StorageName == storageName))
            .GroupBy(x => x.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalCount = g.Sum(x => x.Count)
            })
            .ToDictionaryAsync(x => x.ProductId,
                x => x.TotalCount, cancellationToken);
    }
}