using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Balances;

public class BadTransactionStatusException(string status) : BadRequestException($"Транзакцию со статусом '{status}' нельзя удалять", new { Status = status })
{
    
}