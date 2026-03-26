using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Balances;

public class TransactionNotFoundExcpetion : NotFoundException, ILocalizableException
{
    public TransactionNotFoundExcpetion(Guid transactionId)
        : base(null, new { TransactionId = transactionId })
    {
    }

    public string MessageKey => "transaction.not.found";
    public object[]? Arguments => null;
}