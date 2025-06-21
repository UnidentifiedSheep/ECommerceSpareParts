namespace Core.RabbitMq.Contracts.Sell.CreateSell.CreateTransaction;

public record CreateSellTransactionRequestedEvent(string WhoCreateUserId, string WhoBoughtId, int CurrencyId, 
    decimal TransactionSum, DateTime TransactionDate, Guid SagaId);