namespace Core.RabbitMq.Contracts.Purchase.CreatePurchase.ChangeInventoryCount;

public record ChangeInventoryCountForPurchaseResultEvent(bool IsSuccess, List<int>? StorageContentIds, Guid SagaId, string? Message);