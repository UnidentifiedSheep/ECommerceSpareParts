namespace Main.Application.Interfaces.Services.Storage.StorageContent;

public interface IStorageContentExtractor
{
    
    /// <summary>
    /// Extracts needed counts from storage count by product id
    /// </summary>
    /// <param name="storageName"></param>
    /// <param name="productCounts"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    List<Entities.Storage.StorageContent> ExtractFromStorage(
        string storageName,
        Dictionary<int, int> productCounts,
        CancellationToken cancellationToken = default);
}