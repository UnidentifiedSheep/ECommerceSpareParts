using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Balances;

public class BadTransactionStatusException : BadRequestException
{
    public BadTransactionStatusException(string status) : base($"Транзакцию со статусом '{status}' нельзя удалять", new { Status = status })
    {
    }
}