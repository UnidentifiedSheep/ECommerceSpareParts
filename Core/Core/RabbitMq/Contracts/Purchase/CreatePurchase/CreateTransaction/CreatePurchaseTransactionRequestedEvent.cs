namespace Core.RabbitMq.Contracts.Purchase.CreatePurchase.CreateTransaction;

public record CreatePurchaseTransactionRequestedEvent(string WhoCreateUserId, string SupplierId, int CurrencyId, 
    decimal TransactionSum, DateTime TransactionDate, Guid SagaId);