using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Balances;

public class TransactionNotFoundExcpetion : NotFoundException
{
    [ExampleExceptionValues(false, "0000-0000-0000-0000")]
    public TransactionNotFoundExcpetion(Guid transactionId) : base("Не удалось найти транзакцию", new { TransactionId = transactionId })
    {
    }
}