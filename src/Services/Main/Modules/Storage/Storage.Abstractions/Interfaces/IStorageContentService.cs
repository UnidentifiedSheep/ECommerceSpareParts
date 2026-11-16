using Enums;
using Main.Application.Models.Storage;
using Main.Enums;

namespace Main.Application.Interfaces.Services;

public interface IStorageContentService
{
	// Changes must run in the caller's application transaction.
	// Save prior party and lock changes before a dependent database query.
	Task<IReadOnlyList<StorageLot>> SubtractByProductsAsync(
		IReadOnlyCollection<SubtractProductFromStorageItem> items,
		StorageMovementType movementType,
		CancellationToken cancellationToken = default);

	// Starts from the specified party and continues on the same warehouse if needed.
	Task<IReadOnlyList<StorageLot>> SubtractByContentsAsync(
		IReadOnlyCollection<SubtractStorageContentItem> items,
		StorageMovementType movementType,
		CancellationToken cancellationToken = default);

	Task<IReadOnlyList<StoredLot>> AddAsync(
		IReadOnlyCollection<NewStorageLot> lots,
		string storageCode,
		StorageMovementType movementType,
		CancellationToken cancellationToken = default);

	Task RestoreAsync(
		IReadOnlyCollection<RestoreStorageLot> lots,
		StorageMovementType movementType,
		CancellationToken cancellationToken = default);

	Task<IReadOnlyDictionary<int, StoredLot>> LockAsync(
		IReadOnlyCollection<int> storageContentIds,
		CancellationToken cancellationToken = default);

	Task UpdatePurchaseInfoAsync(
		int storageContentId,
		int currencyId,
		DateTime purchaseDate,
		decimal buyPrice,
		CancellationToken cancellationToken = default);
}
