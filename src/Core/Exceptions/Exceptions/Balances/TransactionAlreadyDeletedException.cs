using Core.Attributes;
using Core.Interfaces.Exceptions;
using Exceptions.Base;

namespace Exceptions.Exceptions.Balances;

public class TransactionAlreadyDeletedException : BadRequestException
{
    [ExampleExceptionValues(false, typeof(TransactionAlreadyDeletedExceptionExample))]
    public TransactionAlreadyDeletedException(Guid transactionId) : base("Транзакция уже удалена", new { TransactionId = transactionId })
    {
    }
}

public class TransactionAlreadyDeletedExceptionExample : IExceptionExample
{
    public Guid TransactionId { get; set; } = Guid.NewGuid();
}