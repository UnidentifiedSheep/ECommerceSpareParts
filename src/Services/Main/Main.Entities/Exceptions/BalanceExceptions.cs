using Exceptions.Base.Localized;
using Main.Enums.Balances;

namespace Main.Entities.Exceptions;

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

public class TransactionSourceCannotBeReversedByUserException(TransactionSourceType sourceType)
    : LocalizedBadRequestException(
        "transaction.source.cannot.be.reversed.by.user",
        new { SourceType = sourceType },
        [sourceType.ToString()]);

public class TransactionWithSystemUserCannotBeCreatedByUserException()
    : LocalizedBadRequestException(
        "transaction.with.system.user.cannot.be.created.by.user");