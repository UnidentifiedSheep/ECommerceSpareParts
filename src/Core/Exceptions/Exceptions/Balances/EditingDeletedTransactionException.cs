using Core.Attributes;
using Core.Interfaces.Exceptions;
using Exceptions.Base;

namespace Exceptions.Exceptions.Balances;

public class EditingDeletedTransactionException : BadRequestException
{
    [ExampleExceptionValues(false, typeof(EditingDeletedTransactionExample))]
    public EditingDeletedTransactionException(Guid transactionId) : base("Нельзя отредактировать удаленную транзакцию", new { TransactionId = transactionId })
    {
    }
}

public class EditingDeletedTransactionExample : IExceptionExample
{
    public Guid TransactionId { get; set; } = Guid.NewGuid();
}