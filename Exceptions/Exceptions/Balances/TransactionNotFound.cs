using Exceptions.Base;

namespace Exceptions.Exceptions.Balances;

public class TransactionNotFound(string transactionId) : NotFoundException($"Не удалось найти транзакцию", new { TransactionId = transactionId })
{
    
}