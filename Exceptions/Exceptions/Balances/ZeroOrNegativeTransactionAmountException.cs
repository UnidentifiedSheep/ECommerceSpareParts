using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Balances;

public class ZeroOrNegativeTransactionAmountException() : BadRequestException("Сумма транзакции не может быть ниже либо равна нулю")
{
    
}