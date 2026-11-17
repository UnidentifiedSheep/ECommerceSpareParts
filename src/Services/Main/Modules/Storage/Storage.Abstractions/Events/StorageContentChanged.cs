using Domain.Interfaces.Events;

namespace Main.Modules.Storage.Abstractions.Events;

public sealed record StorageContentSnapshot(
	int StorageContentId,
	int ProductId,
	string StorageCode,
	int CurrencyId,
	decimal BuyPrice,
	int AvailableCount);

public sealed record StorageContentChanged(StorageContentSnapshot Content, DateTime OccurredAt)
	: IBatchableDomainEvent;
