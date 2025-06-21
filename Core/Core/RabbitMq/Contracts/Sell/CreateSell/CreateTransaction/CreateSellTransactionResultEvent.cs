namespace Core.RabbitMq.Contracts.Sell.CreateSell.CreateTransaction;

public record CreateSellTransactionResultEvent(bool IsSuccess, string? TransactionId, Guid SagaId);