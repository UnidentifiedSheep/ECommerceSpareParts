using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Balances;

public class TransactionNotFound(string transactionId) : NotFoundException($"Не удалось найти транзакцию", new { TransactionId = transactionId })
{
    
}