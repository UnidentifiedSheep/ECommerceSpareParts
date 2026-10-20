using Application.Common.Interfaces.Persistence;
using Application.Common.Models.Persistence;
using Enums;
using Attributes;

namespace Application.Common.Extensions;

public static class ApplicationTransactionExtensions
{
	public static IApplicationTransaction WithSettingsIfIdle(
		this IApplicationTransaction transaction, TransactionalAttribute? settings) =>
		transaction.IsExecuting ? transaction : transaction.WithSettings(settings);

	public static IApplicationTransaction OnException(
		this IApplicationTransaction transaction,
		TransactionErrorKind kind,
		Action<TransactionError> handler)
	{
		ArgumentNullException.ThrowIfNull(transaction);
		ArgumentNullException.ThrowIfNull(handler);
		return transaction.OnException(error =>
		{
			if ((error.Kind & kind) != TransactionErrorKind.None)
				handler(error);
		});
	}
}
