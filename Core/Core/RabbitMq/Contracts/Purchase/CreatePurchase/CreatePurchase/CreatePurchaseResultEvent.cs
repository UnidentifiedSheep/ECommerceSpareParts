namespace Core.RabbitMq.Contracts.Purchase.CreatePurchase.CreatePurchase;

public record CreatePurchaseResultEvent(bool IsSuccess, string? PurchaseId, Guid SagaId, string? Message);