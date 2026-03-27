using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Balances;

public class EditingDeletedTransactionException : BadRequestException, ILocalizableException
{
    public EditingDeletedTransactionException(Guid transactionId) : base(null, new { TransactionId = transactionId })
    {
    }

    public string MessageKey => "deleted.transaction.cannot.be.edited";
    public object[]? Arguments => null;
}