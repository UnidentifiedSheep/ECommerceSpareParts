using Core.Entities;

namespace Core.Interfaces.DbRepositories;

public interface IStorageContentRepository
{
    Task<Dictionary<int, List<decimal>>> GetHighestBuyPrices(IEnumerable<int> articleIds, int takePerArticle, bool calcWhereZero = false, 
        CancellationToken cancellationToken = default);
    Task<IEnumerable<StorageContent>> GetStorageContentsForUpdate(IEnumerable<int> ids, bool track = true, CancellationToken cancellationToken = default);

    Task<IEnumerable<StorageContent>> GetStorageContents(string? storageName, int? articleId, int page, int viewCount,
        bool showZeroCount, bool track = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получение StorageContent, по id и articleId и storageName.
    /// Поля не учитываются если null.
    /// Блокирует строки FOR UPDATE. 
    /// </summary>
    /// <param name="id">storage content id</param>
    /// <param name="articleId">article id</param>
    /// <param name="storageName">storage name</param>
    /// <param name="track">track or not</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<StorageContent?> GetStorageContentForUpdateAsync(int? id, int? articleId, string? storageName, bool track = true, CancellationToken cancellationToken = default);

    Task<Dictionary<(int contentId, string storageId), StorageContent>> GetStorageContentsForUpdateAsync(
        IEnumerable<(int contentId, string storageId)> ids, bool track = true, CancellationToken cancellationToken = default);
    IAsyncEnumerable<StorageContent> GetStorageContentsForUpdateAsync(int? articleId, string? storageName,
        IEnumerable<int>? exceptArticleIds = null, IEnumerable<string>? exceptStorages = null, int countGreaterThen = 0, 
        bool track = true);
    
    Task<Dictionary<int, int>> GetStorageContentCounts(string storageName, IEnumerable<int> articleIds, 
        bool takeFromOtherStorages, CancellationToken cancellationToken = default);
}