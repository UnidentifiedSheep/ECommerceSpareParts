using Exceptions.Base;

namespace Exceptions.Exceptions.Balances;

public class BadTransactionStatusException(string status)
    : BadRequestException($"Транзакцию со статусом '{status}' нельзя удалять", new { Status = status })
{
}