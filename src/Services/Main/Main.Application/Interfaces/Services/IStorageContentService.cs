using Main.Application.Models.Storage;
using Main.Enums;

namespace Main.Application.Interfaces.Services;

public interface IStorageContentService
{
	// Changes must run in the caller's application transaction.
	// Save prior party and lock changes before a dependent database query.
	Task<IReadOnlyList<StorageLot>> SubtractAsync(
		IReadOnlyCollection<ISubtractStorageContentItem> items,
		StorageMovementType movementType,
		CancellationToken cancellationToken = default);

	// Callers must lock the affected storage content rows before checking their quantities.
	Task EnsureCanSetQuantitiesAsync(
		IReadOnlyCollection<StorageContentQuantityChange> changes,
		CancellationToken cancellationToken = default);
}
