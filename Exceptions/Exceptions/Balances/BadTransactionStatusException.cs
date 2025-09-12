using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Balances;

public class BadTransactionStatusException(string status) : BadRequestException($"Транзакцию со статусом '{status}' нельзя удалять", new { Status = status })
{
    
}