using Main.Entities.Storage;

namespace Main.Application.Interfaces.Repositories;

public interface IStorageContentRepository
{
    IAsyncEnumerable<StorageContent> GetStorageContentsForUpdateAsync(
        int? productId,
        string? storageName,
        IEnumerable<int>? exceptProductIds = null,
        IEnumerable<string>? exceptStorages = null,
        int countGreaterThen = 0);
}