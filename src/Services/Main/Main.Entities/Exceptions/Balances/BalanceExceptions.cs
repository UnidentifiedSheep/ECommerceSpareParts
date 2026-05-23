using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions.Balances;

public class BadTransactionStatusException(string status)
    : LocalizedBadRequestException(
        "transaction.invalid.status.for.deletion",
        new { Status = status },
        [status]);

public class EditingDeletedTransactionException(Guid transactionId)
    : LocalizedBadRequestException(
        "deleted.transaction.cannot.be.edited",
        new { TransactionId = transactionId });

public class TransactionAlreadyDeletedException(Guid transactionId)
    : LocalizedBadRequestException(
        "transaction.already.deleted",
        new { TransactionId = transactionId });

public class TransactionNotFoundException(Guid transactionId)
    : LocalizedNotFoundException(
        "transaction.not.found",
        new { TransactionId = transactionId });
