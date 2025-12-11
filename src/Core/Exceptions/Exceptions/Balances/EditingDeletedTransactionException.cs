using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Balances;

public class EditingDeletedTransactionException : BadRequestException
{
    public EditingDeletedTransactionException(Guid transactionId) : base("Нельзя отредактировать удаленную транзакцию", new { TransactionId = transactionId })
    {
    }
}