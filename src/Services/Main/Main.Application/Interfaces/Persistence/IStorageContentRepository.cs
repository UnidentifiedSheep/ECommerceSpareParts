using Application.Common.Interfaces.Repositories;
using Main.Application.NamedObjects.StorageContentExtractPolicies;
using Main.Entities.Storage;

namespace Main.Application.Interfaces.Persistence;

public interface IStorageContentRepository : IRepository<StorageContent, int>
{
    IAsyncEnumerable<StorageContent> GetStorageContentsForUpdateAsync(
        int? productId,
        string? storageName,
        IEnumerable<int>? exceptProductIds = null,
        IEnumerable<string>? exceptStorages = null,
        int countGreaterThen = 0,
        StorageContentExtractPolicyBase? policy = null);

    Task<Dictionary<int, int>> GetStorageContentCounts(
        string storageName,
        IEnumerable<int> productIds,
        bool takeFromOtherStorages,
        CancellationToken cancellationToken = default);
}
