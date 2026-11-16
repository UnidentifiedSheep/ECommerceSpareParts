using Domain.Interfaces.Events;
using Enums;

namespace Main.Entities.Storage.DomainEvents.StorageContent;

public record StorageContentCountUpdatedDomainEvent(
	int ProductId,
	string StorageCode,
	int CurrencyId,
	int NewCount,
	decimal BuyPrice,
	StorageMovementType MovementType,
	int Delta) : IBatchableDomainEvent;
