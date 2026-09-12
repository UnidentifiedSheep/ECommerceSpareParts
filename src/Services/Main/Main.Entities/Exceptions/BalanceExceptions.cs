using Exceptions.Base.Localized;
using Main.Enums.Balances;

namespace Main.Entities.Exceptions;

public class BadTransactionStatusException(string status) : LocalizedBadRequestException(
	new TransactionInvalidStatusForDeletionMessage().WithStatus(status),
	new
	{
		Status = status
	});

public class EditingDeletedTransactionException(Guid transactionId) : LocalizedBadRequestException(
	DeletedTransactionCannotBeEditedMessage.Instance,
	new
	{
		TransactionId = transactionId
	});

public class TransactionAlreadyDeletedException(Guid transactionId) : LocalizedBadRequestException(
	TransactionAlreadyDeletedMessage.Instance,
	new
	{
		TransactionId = transactionId
	});

public class TransactionNotFoundException(Guid transactionId) : LocalizedNotFoundException(
	TransactionNotFoundMessage.Instance,
	new
	{
		TransactionId = transactionId
	});

public class TransactionSourceCannotBeReversedByUserException(TransactionSourceType sourceType)
	: LocalizedBadRequestException(
		new TransactionSourceCannotBeReversedByUserMessage().WithSourceType(sourceType.ToString()),
		new
		{
			SourceType = sourceType
		});

public class TransactionWithSystemOrganizationCannotBeCreatedByUserException()
	: LocalizedBadRequestException(TransactionWithSystemOrganizationCannotBeCreatedByUserMessage.Instance);
