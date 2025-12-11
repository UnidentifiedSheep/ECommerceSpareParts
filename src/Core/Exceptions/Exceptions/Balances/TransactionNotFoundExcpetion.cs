using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Balances;

public class TransactionNotFoundExcpetion : NotFoundException
{
    public TransactionNotFoundExcpetion(Guid transactionId) : base("Не удалось найти транзакцию", new { TransactionId = transactionId })
    {
    }
}