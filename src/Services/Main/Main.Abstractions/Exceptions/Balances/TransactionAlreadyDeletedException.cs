using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Balances;

public class TransactionAlreadyDeletedException : BadRequestException, ILocalizableException
{
    public string MessageKey => "transaction.already.deleted";
    public object[]? Arguments => null;
    public TransactionAlreadyDeletedException(Guid transactionId) 
        : base(null, new { TransactionId = transactionId }) { }
}