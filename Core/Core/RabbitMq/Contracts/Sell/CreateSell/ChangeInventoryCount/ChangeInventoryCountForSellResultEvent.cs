using Core.RabbitMq.Contracts.Sell.CreateSell.Models;

namespace Core.RabbitMq.Contracts.Sell.CreateSell.ChangeInventoryCount;

public record ChangeInventoryCountForSellResultEvent(bool IsSuccess, List<TakeFromStorage>? StorageContentIds, string? Message, Guid SagaId);