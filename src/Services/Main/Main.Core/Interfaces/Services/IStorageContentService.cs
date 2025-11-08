using Main.Core.Entities;

namespace Main.Core.Interfaces.Services;

public interface IStorageContentService
{
    /// <summary>
    /// Gets storage contents for update and validates that all off them exists
    /// </summary>
    /// <param name="storageContentIds">Storage content ids</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Dictionary<int, StorageContent>> GetStorageContentsForUpdate(
        IEnumerable<int> storageContentIds, CancellationToken cancellationToken = default);
}