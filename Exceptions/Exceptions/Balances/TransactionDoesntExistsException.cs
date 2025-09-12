using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Balances;

public class TransactionDoesntExistsException(string key) : NotFoundException($"Не удалось найти транзакцию", new { Key = key })
{
    
}