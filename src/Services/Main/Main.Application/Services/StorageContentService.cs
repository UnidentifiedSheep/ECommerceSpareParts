using Exceptions.Exceptions.Storages;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Abstractions.Interfaces.Services;
using Main.Entities;

namespace Main.Application.Services;

public class StorageContentService(IStorageContentRepository storageContentRepository) : IStorageContentService
{
    public async Task<Dictionary<int, StorageContent>> GetStorageContentsForUpdate(
        IEnumerable<int> storageContentIds, CancellationToken cancellationToken = default)
    {
        var ids = storageContentIds.ToHashSet();
        var storageContents = (await storageContentRepository.GetStorageContentsForUpdate(ids,
            true, cancellationToken)).ToDictionary(x => x.Id);
        if (storageContents.Count != ids.Count)
            throw new StorageContentNotFoundException(ids.Except(storageContents.Keys));
        return storageContents;
    }
}