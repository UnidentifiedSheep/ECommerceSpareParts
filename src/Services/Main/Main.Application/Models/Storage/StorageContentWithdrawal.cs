using Main.Entities.Storage;
using Main.Enums;

namespace Main.Application.Models.Storage;

public sealed record StorageContentWithdrawal(
	IReadOnlyCollection<ISubtractStorageContentItem> Items,
	StorageMovementType MovementType);

public sealed record StorageContentSelection(int ProductId, string? StorageCode);

public sealed record StorageContentQuantityChange(StorageContent Content, int Quantity);
