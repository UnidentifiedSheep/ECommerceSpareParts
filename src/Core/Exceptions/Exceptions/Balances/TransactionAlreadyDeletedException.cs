using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Balances;

public class TransactionAlreadyDeletedException : BadRequestException
{
    public TransactionAlreadyDeletedException(Guid transactionId) : base("Транзакция уже удалена", new { TransactionId = transactionId })
    {
    }
}