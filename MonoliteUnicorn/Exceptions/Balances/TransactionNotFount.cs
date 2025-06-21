using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Balances;

public class TransactionNotFount(string transactionId) : NotFoundException($"Не удалось найти транзакцию {transactionId}")
{
    
}