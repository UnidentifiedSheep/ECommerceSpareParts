using Application.Common.Interfaces.Persistence;
using Application.Common.Models.Persistence;
using Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Persistence.TransactionBuilder;

internal sealed class PostgresTransactionExceptionClassifier : ITransactionExceptionClassifier
{
	public TransactionError Classify(Exception exception)
	{
		var postgres = FindPostgresException(exception);
		var kind = postgres?.SqlState switch
		{
			PostgresErrorCodes.ForeignKeyViolation => TransactionErrorKind.ForeignKeyViolation,
			PostgresErrorCodes.UniqueViolation => TransactionErrorKind.UniqueConstraintViolation,
			PostgresErrorCodes.NotNullViolation => TransactionErrorKind.NotNullViolation,
			PostgresErrorCodes.CheckViolation => TransactionErrorKind.CheckConstraintViolation,
			PostgresErrorCodes.SerializationFailure => TransactionErrorKind.ConcurrencyConflict,
			PostgresErrorCodes.DeadlockDetected => TransactionErrorKind.Deadlock,
			PostgresErrorCodes.LockNotAvailable => TransactionErrorKind.LockUnavailable,
			_ when FindException<DbUpdateConcurrencyException>(exception) is not null => TransactionErrorKind.ConcurrencyConflict,
			_ => TransactionErrorKind.Unknown
		};
		return new TransactionError(exception, kind, postgres?.ConstraintName);
	}

	internal static PostgresException? FindPostgresException(Exception exception) => FindException<PostgresException>(exception);

	private static TException? FindException<TException>(Exception exception) where TException : Exception
	{
		for (var current = exception; current is not null; current = current.InnerException)
			if (current is TException found)
				return found;
		return null;
	}
}
