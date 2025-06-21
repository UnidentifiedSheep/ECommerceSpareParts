namespace Core.RabbitMq.Contracts.Purchase.CreatePurchase.CreateTransaction;

public record CreatePurchaseTransactionResultEvent(bool IsSuccess, string? TransactionId, Guid SagaId, string? Message);