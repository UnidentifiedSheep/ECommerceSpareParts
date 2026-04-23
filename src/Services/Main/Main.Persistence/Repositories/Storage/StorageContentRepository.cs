using Main.Application.Interfaces.Repositories;
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
}