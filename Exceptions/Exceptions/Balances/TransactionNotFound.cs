using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Balances;

public class TransactionNotFound(string transactionId) : NotFoundException($"Не удалось найти транзакцию", new { TransactionId = transactionId })
{
    
}