using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Balances;

public class BadTransactionStatusException : BadRequestException, ILocalizableException
{
    public string MessageKey => "transaction.invalid.status.for.deletion";
    public object[]? Arguments { get; }
    public BadTransactionStatusException(string status) : base(null, new { Status = status })
    {
        Arguments = [status];
    }
}