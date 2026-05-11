using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Entities.Exceptions.Balances;

public class TransactionAlreadyDeletedException : BadRequestException, ILocalizableException
{
    public TransactionAlreadyDeletedException(Guid transactionId)
        : base(null, new { TransactionId = transactionId })
    {
    }

    public string MessageKey => "transaction.already.deleted";
    public object[]? Arguments => null;
}