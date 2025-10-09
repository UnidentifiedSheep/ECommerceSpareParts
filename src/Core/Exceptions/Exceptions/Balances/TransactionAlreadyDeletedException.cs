using Exceptions.Base;

namespace Exceptions.Exceptions.Balances;

public class TransactionAlreadyDeletedException(string transactionId)
    : BadRequestException("Транзакция уже удалена", new { TransactionId = transactionId })
{
}