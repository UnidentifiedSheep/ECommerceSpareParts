using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Balances;

public class SameTransactionExists() : BadRequestException("Транзакция с таким отправителем, получателем и временем отправки уже есть.")
{
    
}