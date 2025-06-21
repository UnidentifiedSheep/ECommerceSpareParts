using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Balances;

public class ZeroOrNegativeTransactionAmountException() : BadRequestException("Сумма транзакции не может быть ниже либо равна нулю")
{
    
}