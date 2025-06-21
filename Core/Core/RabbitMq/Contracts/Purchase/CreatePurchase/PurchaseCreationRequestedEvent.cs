namespace Core.RabbitMq.Contracts.Purchase.CreatePurchase;

public record PurchaseCreationRequestedEvent(string WhoCreatedUserId, string SupplierId, int CurrencyId, Guid SagaId, string StorageName,
    DateTime PurchaseDatetime, string? Comment, string PurchaseContent, decimal PayedSum);