using MonoliteUnicorn.Dtos.Amw.Purchase;
using MonoliteUnicorn.Dtos.Amw.Storage;
using MonoliteUnicorn.Enums;
using MonoliteUnicorn.Models;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.Services.Inventory;

public interface IInventory
{
    Task<IEnumerable<StorageContent>> AddContentToStorage(
        IEnumerable<(int ArticleId, int Count, decimal Price, int currencyId)> content,
        string storageName, string userId, StorageContentStatus status,
        StorageMovementType movementType, CancellationToken cancellationToken = default);

    Task AddContentToStorage(IEnumerable<(SaleContentDetail, int)> contentDetails, StorageMovementType movementType,
        string userId, CancellationToken cancellationToken = default);

    Task DeleteContentFromStorage(int contentId, string userId, StorageMovementType movementType,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<PrevAndNewValue<StorageContent>>> RemoveContentFromStorage(
        IEnumerable<(int ArticleId, int Count)> content,
        string userId, string? storageName, bool takeFromOtherStorages,
        StorageMovementType movementType, CancellationToken cancellationToken = default);

    Task AddOrRemoveContentFromStorage(Dictionary<int, Dictionary<decimal, int>> addRemoveDict,
        int currencyId, string storageName,
        DateTime prevPurchaseDateTime, DateTime newPurchaseDateTime, string userId,
        StorageMovementType movementType, CancellationToken cancellationToken = default);

    Task EditStorageContent(
        Dictionary<int, PatchStorageContentDto> editedFields,
        string userId,
        CancellationToken cancellationToken = default);
}