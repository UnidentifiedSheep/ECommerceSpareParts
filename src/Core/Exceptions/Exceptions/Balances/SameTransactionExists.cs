using Exceptions.Base;

namespace Exceptions.Exceptions.Balances;

public class SameTransactionExists()
    : BadRequestException("Транзакция с таким отправителем, получателем и временем отправки уже есть.")
{
}