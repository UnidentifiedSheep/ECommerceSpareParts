using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Balances;

public class TransactionAlreadyDeletedException(string transactionId) : BadRequestException($"Транзакция '{transactionId}' уже удалена")
{
    
}