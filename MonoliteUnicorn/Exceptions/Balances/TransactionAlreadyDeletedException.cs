using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Balances;

public class TransactionAlreadyDeletedException(string transactionId) : BadRequestException($"Транзакция уже удалена", new { TransactionId = transactionId })
{
    
}