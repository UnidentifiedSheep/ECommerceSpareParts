using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Balances;

public class EditingDeletedTransactionException(string transactionId) : 
    BadRequestException($"Нельзя отредактировать удаленную транзакцию", new { TransactionId = transactionId })
{
    
}