using Exceptions.Base;

namespace Exceptions.Exceptions.Balances;

public class EditingDeletedTransactionException(string transactionId) :
    BadRequestException("Нельзя отредактировать удаленную транзакцию", new { TransactionId = transactionId })
{
}