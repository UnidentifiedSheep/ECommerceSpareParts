using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Balances;

public class TransactionAlreadyDeletedException(string transactionId) : BadRequestException($"Транзакция уже удалена", new { TransactionId = transactionId })
{
    
}