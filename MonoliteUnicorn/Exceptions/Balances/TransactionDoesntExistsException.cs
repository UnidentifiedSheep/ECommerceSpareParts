using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Balances;

public class TransactionDoesntExistsException(string key) : NotFoundException($"Не удалось найти транзакцию {key}")
{
    
}