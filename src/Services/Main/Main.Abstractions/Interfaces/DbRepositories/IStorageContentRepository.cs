using System.Linq.Expressions;
using Main.Abstractions.Models;
using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IStorageContentRepository
{
    Task<List<StorageContentPriceProjection>> GetStorageContentPricingInfo(IEnumerable<int> articleIds, 
        bool onlyPositiveQty = true, CancellationToken ct = default);

    Task<IEnumerable<StorageContent>> GetStorageContentsForUpdate(IEnumerable<int> ids, bool track = true,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<StorageContent>> GetStorageContents(string? storageName, int? articleId, int page, int viewCount,
        bool showZeroCount, bool track = true, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Получение StorageContent, по id и articleId и storageName.
    ///     Поля не учитываются если null.
    ///     Блокирует строки FOR UPDATE.
    /// </summary>
    /// <param name="id">storage content id</param>
    /// <param name="articleId">article id</param>
    /// <param name="storageName">storage name</param>
    /// <param name="track">track or not</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<StorageContent?> GetStorageContentForUpdateAsync(int? id, int? articleId, string? storageName,
        bool track = true, CancellationToken cancellationToken = default);

    Task<Dictionary<(int contentId, string storageId), StorageContent>> GetStorageContentsForUpdateAsync(
        IEnumerable<(int contentId, string storageId)> ids, bool track = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получение StorageContent, по articleId и storageName. Сортировка идет по дате покупки (по возрастанию).
    /// </summary>
    /// <param name="articleId">Айди артикула</param>
    /// <param name="storageName">Название склада</param>
    /// <param name="exceptArticleIds">Артикулы айди которые не должны быть в результате</param>
    /// <param name="exceptStorages">Склады которые не должны быть в результате</param>
    /// <param name="countGreaterThen">Число больше которого должно быть Количество позиции</param>
    /// <param name="track">Отслеживать сущность/и или нет</param>
    /// <returns></returns>
    IAsyncEnumerable<StorageContent> GetStorageContentsForUpdateAsync(int? articleId, string? storageName,
        IEnumerable<int>? exceptArticleIds = null, IEnumerable<string>? exceptStorages = null, int countGreaterThen = 0,
        bool track = true);

    Task<Dictionary<int, int>> GetStorageContentCounts(string storageName, IEnumerable<int> articleIds,
        bool takeFromOtherStorages, CancellationToken cancellationToken = default);
}