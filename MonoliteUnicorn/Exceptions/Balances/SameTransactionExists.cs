using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Balances;

public class SameTransactionExists() : BadRequestException("Транзакция с таким отправителем, получателем и временем отправки уже есть.")
{
    
}