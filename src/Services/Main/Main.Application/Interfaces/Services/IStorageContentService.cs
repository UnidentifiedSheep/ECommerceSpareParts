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

}
